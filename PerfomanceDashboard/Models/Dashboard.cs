using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using Microsoft.Exchange.WebServices.Data;
using System.Net;

namespace PerfomanceDashboard.Models
{
    public static class Dashboard
    {
        private static Dictionary<string, string> nameToShortName = new Dictionary<string, string>();
        
public static bool actionTrigger = false;

        private static int unassignedTrigger = unassignedTicket;
        public static int backlog, unassignedTicket, lastFhr;

        public static string pereodicalBody;

        public static List<string> emplNames, emplNamesShort;

        public static List<string[]> assignedWithStatusByPerson = new List<string[]>();
        public static List<string[]> resolvedTicketPerWeekByUser = new List<string[]>();
        public static List<int[]> assignedToGroupByDayOfWeek = new List<int[]>();
        public static List<string[]> fhrTable = new List<string[]>();
        public static List<DataTable> rawTable = new List<DataTable>();

        private static string RawToGoodViewConverter(string data) // ← parse only first table from email
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(data);
            var node = doc.DocumentNode.SelectSingleNode("//table[@class='MsoNormalTable']");
            if (node != null)
            {
                var OuterHtml = node.OuterHtml;
                data = OuterHtml;
            }
            return data;
        }

        public static string ExchangeConnectorToServiceMailbox() // ← get raw body of the last email contains "PEREODICAL" in subject
        {
            string result = "";

            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);
            service.Credentials = new NetworkCredential("svc_toEusDashboard", "CDYLamp!dH0Z52nNw", "KL");
            service.Url = new Uri("https://hqoutlook.avp.ru/EWS/Exchange.asmx");

            FindItemsResults<Item> findResults = service.FindItems(WellKnownFolderName.Inbox, new ItemView(1));

            foreach (Item item in findResults.Items)
            {
                if (item.Subject.Contains("PERIODICAL"))
                {
                    item.Load();
                    result = item.Body.Text;
                    return RawToGoodViewConverter(result);
                }
                else return "No one item in mailbox contains mail with subject PEREODICAL";
            }
            return RawToGoodViewConverter(result);
        }

        public static void ActionTrigger()
        {
            if (unassignedTrigger != unassignedTicket)
            {
                unassignedTrigger = unassignedTicket;
                actionTrigger = true;
            }
            else
                actionTrigger = false;
        }

        public static void GetDataFromDb()
        {

            // → create connection string, data-set variables and query list
            string cs = ConfigurationManager.ConnectionStrings["ConnStringDb1"].ConnectionString;
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            List<string> query = new List<string>();

            // clear
            query.Clear();
            resolvedTicketPerWeekByUser.Clear();
            rawTable.Clear();
            assignedWithStatusByPerson.Clear();
            nameToShortName.Clear();
            assignedToGroupByDayOfWeek.Clear();
            fhrTable.Clear();

            // → current backlog
            query.Add(@"SELECT * 
                            FROM   [ARSystem].[dbo].[hpd_help_desk] 
                            WHERE  assigned_group = 'SD EUS' 
		                            AND status BETWEEN 1 AND 3 
		                            AND (Status_Reason = 8000
		                            OR Status_Reason IS NULL);");

            // → current unisigned
            query.Add(@"SELECT Incident_Number,  
                                   Max(last_modified_date2) 
                            FROM   [ARSystem].[dbo].[hpd_search_assignment_logs]  
                            WHERE  assigned_group = 'SD EUS'  
                                   AND status BETWEEN 1 AND 3 
	                               AND Assignee IS NULL 
                            GROUP  BY Incident_Number");

            // → FHR tickets
            query.Add(@"DECLARE @curentTimestamp int;
                            SET @curentTimestamp = DATEDIFF(s, '1970-01-01 00:00:00', GETUTCDATE());

                            SELECT CASE 
			                            WHEN submit_date IS NOT NULL THEN 60 - (((@curentTimestamp - submit_date) / 60))
		                            END       AS Left_Minutes, 
		                            assignee 'Assogned To', 

                                    full_name 'Affected User',
                                    description 'Description',
                                    incident_number 'Incident Number',
                                    Max(last_modified_date) 'Last Modified Date'
                            FROM[ARSystem].[dbo].[hpd_help_desk]
                            WHERE  ksp_fhr = 0

                                    AND assigned_group = 'SD EUS'

                                    AND status BETWEEN 1 AND 3
                            GROUP  BY assignee,
                                        full_name,
                                        submit_date,
                                        description,
                                        incident_number");

            // → assigned to person by status
            query.Add(@"IF Object_id(N'tempdb..#TempAssignedFull', 'U') IS NOT NULL 
                                BEGIN 
                                    TRUNCATE TABLE #tempassignedfull 
                                END 
                            ELSE 
                                BEGIN 
                                    CREATE TABLE #tempassignedfull 
                                    ( 
                                        assignee_login_id NVARCHAR(254) NULL, 
                                        assignee          NVARCHAR(69) NULL, 
                                        incindent_number  NVARCHAR(15) NULL, 
                                        lastmodified      INT NOT NULL, 
                                        status            INT NULL, 
                                        status_reason     INT NULL 
                                    ) 
                                END; 

                            IF Object_id(N'tempdb..#TempAssignedResult', 'U') IS NOT NULL 
                                BEGIN 
                                    TRUNCATE TABLE #tempassignedresult 
                                END 
                            ELSE 
                                BEGIN 
                                    CREATE TABLE #tempassignedresult 
                                    ( 
                                        fullname     NVARCHAR(69), 
                                        inprogress   INT, 
                                        future       INT, 
                                        clientaction INT, 
                                        vendorreply  INT 
                                    ) 
                                END; 

                            INSERT INTO #tempassignedfull 
                            SELECT assignee_login_id, 
                                    assignee, 
                                    incident_number, 
                                    Max(last_modified_date2) LastModified, 
                                    status, 
                                    status_reason 
                            FROM   hpd_search_assignment_logs 
                            WHERE  ( assigned_group = 'SD EUS' ) 
                                    AND ( status BETWEEN 1 AND 3 ) 
                                    AND ( assignee IS NOT NULL ) 
                            GROUP  BY assignee_login_id, 
                                        assignee, 
                                        incident_number, 
                                        status, 
                                        status_reason 
                            ORDER  BY assignee 

                            DECLARE @curData CURSOR 
                            DECLARE @strLogin NVARCHAR(254) 
                            DECLARE @strFullName NVARCHAR(69) 
                            DECLARE @intInProgress INT 
                            DECLARE @intFuture INT 
                            DECLARE @intClientAction INT 
                            DECLARE @intVendorReply INT 

                            SET @curData = CURSOR scroll 
                            FOR SELECT DISTINCT assignee_login_id, 
                                                assignee 
                                FROM   #tempassignedfull 

                            OPEN @curData 

                            FETCH next FROM @curData INTO @strLogin, @strFullName 

                            WHILE @@FETCH_STATUS = 0 
                                BEGIN 
                                    -- In progress 
                                    SET @intInProgress = (SELECT DISTINCT Count(*) 
                                                        FROM   #tempassignedfull 
                                                        WHERE  ( assignee_login_id = @strLogin ) 
                                                                AND ( ( status = 1 ) 
                                                                        OR ( status = 2 ) )) 
                                    -- Future 
                                    SET @intFuture = (SELECT DISTINCT Count(*) 
                                                    FROM   #tempassignedfull 
                                                    WHERE  ( assignee_login_id = @strLogin ) 
                                                            AND ( status = 3 ) 
                                                            AND ( status_reason = 11000 )) 
                                    -- Client Action 
                                    SET @intClientAction = (SELECT DISTINCT Count(*) 
                                                            FROM   #tempassignedfull 
                                                            WHERE  ( assignee_login_id = @strLogin ) 
                                                                    AND ( status = 3 ) 
                                                                    AND ( status_reason = 8000 )) 
                                    -- Waiting for vendor reply 
                                    SET @intVendorReply = (SELECT DISTINCT Count(*) 
                                                            FROM   #tempassignedfull 
                                                            WHERE  ( assignee_login_id = @strLogin ) 
                                                                AND ( status = 3 ) 
                                                                AND ( status_reason = 93000 )) 

                                    INSERT INTO #tempassignedresult 
                                    VALUES      (@strFullName, 
                                                @intInProgress, 
                                                @intFuture, 
                                                @intClientAction, 
                                                @intVendorReply) 

                                    FETCH next FROM @curData INTO @strLogin, @strFullName 
                                END 

                            CLOSE @curData 

                            DEALLOCATE @curData 

                            SELECT fullname, 
                                    inprogress, 
                                    future, 
                                    clientaction, 
                                    vendorreply 
                            FROM   #tempassignedresult 
                            ORDER  BY inprogress 

                            DROP TABLE #tempassignedfull 
                            DROP TABLE #tempassignedresult");

            // → resolved per week
            query.Add(@"DECLARE @beginOfWeek int; 
                        DECLARE @endOfWeek int; 
                        DECLARE @tempTable table
                        (
	                        emplName nvarchar(255) null,
	                        last_modified_date2 nvarchar(255) null,
	                        incident nvarchar(255) null
                        );

                        SET @beginOfWeek = DATEDIFF(s, '1970-01-01 00:00:00', dateadd(wk, datediff(wk, 0, getdate()), 0)) - 105637; 
                        SET @endOfWeek = DATEDIFF(s, '1970-01-01 00:00:00', dateadd(wk, datediff(wk, 0, getdate()), 0) + 6); 


                        INSERT INTO @tempTable (emplName, last_modified_date2, incident)
		                        SELECT assignee,    
			                           Max(last_modified_date2),
			                           Incident_Number
		                        FROM   hpd_search_assignment_logs  
		                        WHERE  assigned_group = 'SD EUS'  
			                           AND status BETWEEN 4 AND 5
			                           AND Last_Modified_Date2 BETWEEN @beginOfWeek and @endOfWeek
                                       AND assignee IS NOT NULL
		                        GROUP  BY assignee,  
				                          incident_number,  
				                          status,  
				                          status_reason

                        SELECT emplname, COUNT(incident) 'Resolved' FROM @tempTable
                        GROUP BY emplname
                        ORDER BY Resolved DESC");

            // → assigned to group in the week
            query.Add(@"DECLARE @startOfDay int; 
                        DECLARE @endOfDay int; 
                        DECLARE @maxId int;
                        DECLARE @tempTable table
                        (
	                        IncidentNumber nvarchar(15) null,
	                        LastModifiedData int
                        )
                        DECLARE @resultTable table 
                        (
	                        ticketCount int
                        )

                        SET @maxId = -1;

                        WHILE @maxId < 11
                          BEGIN
	                        SET @maxId = @maxId + 1;
	                        SET @startOfDay = DATEDIFF(s, '1970-01-01 00:00:00', DATEADD(DAY, DATEDIFF(DAY, '19000101', GETDATE()), '19000101') - @maxId);
	                        SET @endOfDay = DATEDIFF(s, '1970-01-01 00:00:00', DATEADD(DAY, DATEDIFF(DAY, '19000101', GETDATE()), '19000101') + 1 - @maxId);
	
	                        INSERT INTO @tempTable (IncidentNumber, LastModifiedData)
	                        SELECT Incident_Number, max(Last_Modified_Date2) 'Last_Modified_Date'

                            FROM hpd_search_assignment_logs

                            WHERE assigned_group = 'SD EUS'

                                    AND Submit_Date BETWEEN @startOfDay AND @endOfDay

                            GROUP BY Incident_Number
                            ORDER BY Last_Modified_Date
                            INSERT INTO @resultTable(ticketCount)
                            SELECT COUNT(*) FROM @tempTable;
                                    DELETE FROM @tempTable
                                  END
                        SELECT* FROM @resultTable");

            // → get raw data
            for (int i = 0; i < query.Count; i++)
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query[i], cs);
                adapter.Fill(ds, "query[" + i + "]");
                rawTable.Add(ds.Tables["query[" + i + "]"]);
            }

            // → convert raw selects to pretty look in order to pass data to front-end beautifuly 
            // → backlog
            backlog = rawTable[0].Rows.Count;

            // → unassigned tickets
            unassignedTicket = rawTable[1].Rows.Count;

            // → most lower FHR
            try
            {
                lastFhr = (rawTable[2].AsEnumerable().Select(r => r.Field<int>("Left_Minutes")).ToList()).Where(a => a > 0 && a < 60).Min();
            }
            catch (Exception e)
            {
                lastFhr = -1;
            }

            // → FHR table
            for (var i = 0; i < rawTable[2].Rows.Count; i++)
            {
                fhrTable.Add(rawTable[2].Rows[i].ItemArray.Select(x => x.ToString()).ToArray());
            }

            // → employeer's names and short names in order to escape dublicates
            emplNames = rawTable[3].AsEnumerable().Select(r => r.Field<string>("fullname")).ToList();
            emplNamesShort = emplNames.Select(x => x.Remove(0, x.IndexOf(" ")).Insert(0, x[0] + ".")).ToList();
            for (var i = 0; i < rawTable[3].Rows.Count; i++)
            {
                assignedWithStatusByPerson.Add(rawTable[3].Rows[i].ItemArray.Select(x => x.ToString()).ToArray());
                if (nameToShortName.Count != emplNames.Count)
                {
                    nameToShortName.Add(emplNames[i], emplNamesShort[i]);
                }
                    
            }

            // → resolved tickets by person
            for (var i = 0; i < rawTable[4].Rows.Count; i++)
            {
                resolvedTicketPerWeekByUser.Add(rawTable[4].Rows[i].ItemArray.Select(x => x.ToString()).ToArray());
                nameToShortName.TryGetValue(resolvedTicketPerWeekByUser[i][0], out resolvedTicketPerWeekByUser[i][0]);
            }

            // → assigned to group
            for (var i = 0; i < rawTable[5].Rows.Count; i++)
            {
                assignedToGroupByDayOfWeek.Add((rawTable[5].Rows[i].ItemArray.Select(x => Convert.ToInt32(x.ToString())).ToArray()));
            }

        }

    }
}