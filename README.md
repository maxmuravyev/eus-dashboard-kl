# eus-dashboard-kl

<h3>What?</h3>
The simple 2-pages dashboard for End User Support group with Kaspersky Lab.

<h3>How?</h3>
jQuery, LESS, <a href="https://github.com/Imaginea/uvCharts">uvChart</a>, <a href="https://github.com/daneden/animate.css/">Animate.css</a>, <a href="https://github.com/elrumordelaluz/csshake">CSShake</a>.

C# includes <a href="https://github.com/OfficeDev/ews-managed-api">EWS</a> and <a href="https://github.com/NuGet/WebBackgrounder">WebBackgrounder</a> with ASP.NET MVC and MS-SQL.

In <b>Global.asax</b> we using WebBackgrounder and check database every 60 seconds. In the <b>Model</b> we fill data with SQL via DataSet. Further we using EWS in order to get data from mailbox by using WebBackgrounder too, parse it all and represent summarized data in <b>Views</b>. Unforch, <b>Controller</b> only for passing data from back to front.



