# eus-dashboard-kl

The simple 2-pages dashboard for End User Support group with Kaspersky Lab.

<h4>Front-End</h4> 
jQuery, LESS, <a href="https://github.com/Imaginea/uvCharts">uvChart</a>, <a href="https://github.com/daneden/animate.css/">Animate.css</a>, <a href="https://github.com/elrumordelaluz/csshake">CSShake</a>.

<h4>Back-End</h4> 
C# includes <a href="https://github.com/OfficeDev/ews-managed-api">EWS</a> and <a href="https://github.com/NuGet/WebBackgrounder">WebBackgrounder</a> with ASP.NET MVC and MS-SQL.

<h3>How it works?</h3>
In <b>Global.asax</b> we using WebBackgrounder and check database every 60 seconds. In the <b>Model</b> we fill data with SQL via DataSet. Further we using EWS in order to get data from mailbox by using WebBackgrounder too, parse it all and represent summarized data in <b>Views</b>. Unforch, <b>Controller</b> only for passing data from back to front.



