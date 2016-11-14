# eus-dashboard-kl

<h3>About</h3>
Simple two-page dashboard for End User Support group of service desk with Kaspersky Lab.

<h4>Front</h4> 
jQuery, LESS, <a href="https://github.com/Imaginea/uvCharts">uvChart</a>, <a href="https://github.com/daneden/animate.css/">Animate.css</a>, <a href="https://github.com/elrumordelaluz/csshake">CSShake</a>.
<h4>Back</h4> 
C# includes <a href="https://github.com/OfficeDev/ews-managed-api">EWS</a> and <a href="https://github.com/NuGet/WebBackgrounder">WebBackgrounder</a> with ASP.NET MVC and MS-SQL.

--

<h3>Recommended Resolution</h3>
Full support for 1920x1080, 1680x1050.
<br/>
Main page support (slider is off by default) for 1366x768, 1280x1024, 1280x800.

<h3>How does it works?</h3>
In <b>Global.asax</b> we using WebBackgrounder and check database every 60 seconds. In the <b>Model</b> we fill data with SQL via DataSet. Further we using EWS in order to get data from mailbox by using WebBackgrounder too, parse it all and represent summarized data in <b>Views</b>. Unforch, <b>Controller</b> only for passing data from back to front.

<h3>Screenshots</h3>
<img src="http://gifyu.com/images/defaulte6550.gif" />


<h3>To Do</h3>
Please <a href="https://github.com/maxmuravyev/eus-dashboard-kl/wiki">use<a>.



