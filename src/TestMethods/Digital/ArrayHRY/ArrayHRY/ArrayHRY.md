# ArrayHRY Template


| Contents      |    
| :----------- |  
| 1. [Introduction](#introduction)      |   
| 2. [Test Instance Parameters](#test-instance-parameters)   |   
| 3. [Configuration File](#configuration-file)   |   
| 3.1. [Changes from iCHSR](#configuration-file-changes-from-ichsr)   |   
| 3.2. [Examples](#configuration-file-examples)   |
| 3.2.1 [Configuration input file example](#configuration-input-file-example) |    
| 3.2.2 [Configuration input file example with condition fail keys](#configuration-input-file-example-with-condition-fail-keys)    
| 3.2.3 [Configuration input file example using fixed length feature](#configuration-input-file-example-using-fixed-length-feature)   
| 3.2.4 [Configuration input file example with bypass global prefix](#configuration-input-file-example-with-bypass-global-prefix)   
| 3.2.5 [Configuration input file example multiple domains](#configuration-input-file-example-multiple-domains)      
| 4. [Optional Features](#optional-features)   |   
| 4.1 [Raw String Forwarding Mode](#raw-string-forwarding-mode)   |   
| 4.2 [Raw string output per condition fail](#raw-string-output-per-condition-fail)   |   
| 4.3 [Fixed length raw string](#fixed-length-raw-string)   |   
| 5. [TPL Samples](#tpl-samples)   |   
| 6. [ITUFF datalog](#ituff-datalog)   |  
| 7. [Exit Ports](#exit-ports)   |  

###

----  
## 1. Introduction   
This template implements the HRY mode of the iCHSR template.  
It has 2 main functions:  
   - Generate HRY raw string from a single Plist execution.  
   - Act as PreScreen test to determine which Raster instance will be executed down the flow. The flow control is done by setting relevant bypass global’s that are specified in the HRY configuration file, and later on used by Raster instance (in bypass_instance/BypassInstance parameter).  


----  
## 2. Test Instance Parameters  

| Parameter Name           | Required? | Type | Description |  
| :-----------             | :----------- | :----------- | :----------- |   
| Patlist                  | Yes | Plist         |  The pattern list to execute. |  
| TimingsTc                | Yes | TestCondition |  The timings testcondition to use. |  
| LevelsTc                 | Yes | TestCondition |  The levels testcondition to use. |  
| PrePlist                 | No  | String | The prime callback to execute before running the plist. |  
| MaskPins                 | No  | Comma-separated String | The pins to mask. |  
| ConfigFile               | Yes | File   | Configuration File to use. See [Configuration File](#configuration-file) for details. |  
| RawStringForwardingMode  | No  | Enum   | PRE (default) or POST. See [Raw String Forwarding Mode](#raw-string-forwarding-mode) for details. |  
| SharedStorageKey         | No  | String | The Prime shared storage key to store the raw hry string (actual key is this + "_" + algorithm name). Uses the String table in DUT context. See [RawStringForwardingMode](#raw-string-forwarding-mode) for details. |  

----  
## 3. Configuration File

[](#this-is-the-table-from-the-iCHSR-documentation-I-didn't-want-to-redo-it-in-markdown-so-I-just-saved-it-as-HTML-and-stuck-it-here...sorry)

<table class=MsoTableGrid border=1 cellspacing=0 cellpadding=0
 style='margin-left:.5in;border-collapse:collapse;border:none;mso-border-alt:
 solid windowtext .5pt;mso-yfti-tbllook:1184;mso-padding-alt:0in 5.4pt 0in 5.4pt'>
 <tr style='mso-yfti-irow:0;mso-yfti-firstrow:yes'>
  <td width=166 style='width:124.65pt;border:inset teal 1.0pt;mso-border-alt:
  inset teal .75pt;background:blue;padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><b><span
  style='mso-bidi-font-size:10.0pt;mso-fareast-font-family:"Times New Roman";
  mso-bidi-font-family:Arial'>Parent Element<o:p></o:p></span></b></p>
  </td>
  <td width=197 style='width:2.05in;border:inset teal 1.0pt;border-left:none;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  background:blue;padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><b><span
  style='mso-bidi-font-size:10.0pt;mso-fareast-font-family:"Times New Roman";
  mso-bidi-font-family:Arial;color:white;mso-color-alt:windowtext'>Element</span></b><span
  style='mso-fareast-font-family:"Times New Roman"'><o:p></o:p></span></p>
  </td>
  <td width=100 valign=top style='width:74.75pt;border:inset teal 1.0pt;
  border-left:none;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  background:blue;padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><strong><span
  style='mso-bidi-font-size:10.0pt;font-family:"Verdana",sans-serif;mso-bidi-font-family:
  Arial;color:white;mso-color-alt:windowtext'>Allowed Repetitions</span></strong><strong><span
  style='mso-bidi-font-size:10.0pt;font-family:"Verdana",sans-serif;mso-bidi-font-family:
  Arial'><o:p></o:p></span></strong></p>
  </td>
  <td width=212 style='width:158.7pt;border:inset teal 1.0pt;border-left:none;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  background:blue;padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='mso-margin-top-alt:auto;mso-margin-bottom-alt:
  auto;text-align:center'><b style='mso-bidi-font-weight:normal'><span
  style='color:white;mso-color-alt:windowtext'>Attribute</span><o:p></o:p></b></p>
  </td>
  <td width=91 valign=top style='width:68.3pt;border:inset teal 1.0pt;
  border-left:none;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  background:blue;padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><strong><span
  style='mso-bidi-font-size:10.0pt;font-family:"Verdana",sans-serif;mso-bidi-font-family:
  Arial;color:white;mso-color-alt:windowtext'>Required/ Optional</span></strong><strong><span
  style='mso-bidi-font-size:10.0pt;font-family:"Verdana",sans-serif;mso-bidi-font-family:
  Arial'><o:p></o:p></span></strong></p>
  </td>
  <td width=566 style='width:424.6pt;border:inset teal 1.0pt;border-left:none;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  background:blue;padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><strong><span
  style='mso-bidi-font-size:10.0pt;font-family:"Verdana",sans-serif;mso-bidi-font-family:
  Arial;color:white;mso-color-alt:windowtext'>Description</span></strong><span
  style='mso-fareast-font-family:"Times New Roman"'><o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:1'>
  <td width=166 style='width:124.65pt;border:inset teal 1.0pt;border-top:none;
  mso-border-top-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='mso-margin-top-alt:auto;mso-margin-bottom-alt:
  auto;text-align:center'><span style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:
  Arial'><o:p>&nbsp;</o:p></span></p>
  </td>
  <td width=197 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>&lt;ReverseCtvCaptureData&gt;<o:p></o:p></span></p>
  </td>
  <td width=100 style='width:74.75pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Only once<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'><o:p>&nbsp;</o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Specify
  if CTV capture data should be reversed before analyzing it.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Allowed
  value is either: Y or N <o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:2'>
  <td width=166 rowspan=2 style='width:124.65pt;border:inset teal 1.0pt;
  border-top:none;mso-border-top-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><span
  style='mso-fareast-font-family:"Times New Roman"'>&lt;CtvToHryMapping&gt;<o:p></o:p></span></p>
  </td>
  <td width=197 rowspan=2 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>&lt;Map&gt;<o:p></o:p></span></p>
  </td>
  <td width=100 rowspan=2 style='width:74.75pt;border-top:none;border-left:
  none;border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Multiple
  times<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>ctv_data<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>The
  CTV char to map to HRY raw string char.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:
  Arial'>Value Uniqueness is validated.</span><span style='mso-fareast-font-family:
  "Times New Roman"'><o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:3'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>hry_data<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>The
  HRY raw string char that is mapped to the CTV char.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Note
  that ‘<b style='mso-bidi-font-weight:normal'>9</b>’ is not a valid value as
  it is reserved for HRY fixed length feature.<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:4'>
  <td width=166 rowspan=4 style='width:124.65pt;border-top:none;border-left:
  inset teal 1.0pt;border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  mso-border-bottom-alt:solid windowtext .5pt;padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><span
  style='mso-fareast-font-family:"Times New Roman"'>&lt;HryPrePostMapping&gt;<o:p></o:p></span></p>
  </td>
  <td width=197 rowspan=2 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>&lt;Map&gt;<o:p></o:p></span></p>
  </td>
  <td width=100 rowspan=2 style='width:74.75pt;border-top:none;border-left:
  none;border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-fareast-font-family:"Times New Roman"'>Multiple times (minimum of
  two)<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>hry_data<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>The
  HRY raw string char to map to status.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Value
  Uniqueness is validated.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Note
  that ‘<b style='mso-bidi-font-weight:normal'>9</b>’ is not a valid value as
  it is reserved for HRY fixed length feature.<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:5'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>status<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>The
  status that is mapped to the HRY raw string char.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Allowed
  value is either: pass OR fail</span><span style='mso-bidi-font-size:10.0pt;
  mso-bidi-font-family:Arial'><o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:6'>
  <td width=197 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>&lt;PostRepairSymbol&gt;<o:p></o:p></span></p>
  </td>
  <td width=100 style='width:74.75pt;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-fareast-font-family:"Times New Roman"'>Only once<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>symbol<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>The
  HRY raw string symbol to use in case repair was identified on POST.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Note
  that ‘<b style='mso-bidi-font-weight:normal'>9</b>’ is not a valid value as
  it is reserved for HRY fixed length feature.<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:7'>
  <td width=197 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'><o:p>&nbsp;</o:p></span></p>
  </td>
  <td width=100 style='width:74.75pt;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-fareast-font-family:"Times New Roman"'>Only once<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'><o:p>&nbsp;</o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Optional<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><b
  style='mso-bidi-font-weight:normal'><span style='mso-fareast-font-family:
  "Times New Roman"'>Note that the element is optional but if specified it must
  contain value.</span></b><span style='mso-fareast-font-family:"Times New Roman"'><o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:8'>
  <td width=166 rowspan=4 style='width:124.65pt;border-top:none;border-left:
  inset teal 1.0pt;border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  mso-border-bottom-alt:solid windowtext .5pt;padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><span
  style='mso-fareast-font-family:"Times New Roman"'>&lt;ConditionFailKeys&gt;<o:p></o:p></span></p>
  </td>
  <td width=197 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>&lt;ConditionFailKey&gt;<o:p></o:p></span></p>
  </td>
  <td width=100 style='width:74.75pt;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-fareast-font-family:"Times New Roman"'>Multiple times<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>name<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>The
  name of the key.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Uniqueness
  between keys is validated.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>If
  key isn’t declared, a verify error will be issued.<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:9'>
  <td width=197 rowspan=2 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-fareast-font-family:"Times New Roman"'>&lt;Map&gt;</span><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'><o:p></o:p></span></p>
  </td>
  <td width=100 rowspan=2 style='width:74.75pt;border-top:none;border-left:
  none;border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-fareast-font-family:"Times New Roman"'>Multiple times<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>expected_data<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Expected
  data to be mapped.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Uniqueness
  between expected data in specific key is validated.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>If
  expected data wasn’t specified, “hry_output_on_condition_fail” from the
  criteria definition will be used.</span><span style='mso-bidi-font-size:10.0pt;
  mso-bidi-font-family:Arial'><o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:10'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>hry_output<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Character
  of HRY raw string output.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Note
  that ‘<b style='mso-bidi-font-weight:normal'>9</b>’ is not a valid value as
  it is reserved for HRY fixed length feature.<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:11'>
  <td width=197 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-fareast-font-family:"Times New Roman"'><o:p>&nbsp;</o:p></span></p>
  </td>
  <td width=100 style='width:74.75pt;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-fareast-font-family:"Times New Roman"'>Only once<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'><o:p>&nbsp;</o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Optional<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><b
  style='mso-bidi-font-weight:normal'><span style='mso-fareast-font-family:
  "Times New Roman"'>Note that the element is optional but if specified it must
  contain value.</span></b><span style='mso-fareast-font-family:"Times New Roman"'><o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:12'>
  <td width=166 style='width:124.65pt;border-top:none;border-left:inset teal 1.0pt;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  mso-border-bottom-alt:solid windowtext .5pt;padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><span
  style='mso-fareast-font-family:"Times New Roman"'><o:p>&nbsp;</o:p></span></p>
  </td>
  <td width=197 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>&lt;BypassGlobalPrefix&gt;<o:p></o:p></span></p>
  </td>
  <td width=100 style='width:74.75pt;border-top:none;border-left:none;
  border-bottom:solid windowtext 1.0pt;border-right:inset teal 1.0pt;
  mso-border-top-alt:inset teal .75pt;mso-border-left-alt:inset teal .75pt;
  mso-border-alt:inset teal .75pt;mso-border-bottom-alt:solid windowtext .5pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Only once<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'><o:p>&nbsp;</o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Optional<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>This
  optional prefix will be added before each of the bypass_global names of each
  &lt;Criteria&gt; defined inside &lt;Criterias&gt; section. <span
  style='mso-spacerun:yes'> </span><o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:13'>
  <td width=166 rowspan=6 style='width:124.65pt;border:inset teal 1.0pt;
  border-top:none;mso-border-top-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><span
  style='mso-fareast-font-family:"Times New Roman"'>&lt;Criterias&gt;<o:p></o:p></span></p>
  </td>
  <td width=197 rowspan=6 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>&lt;Criteria&gt;<o:p></o:p></span></p>
  </td>
  <td width=100 rowspan=6 style='width:74.75pt;border-top:none;border-left:
  none;border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-fareast-font-family:"Times New Roman"'>Multiple times</span><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'><o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>hry_index<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Specify
  either of:<o:p></o:p></span></p>
  <p class=MsoListParagraphCxSpFirst style='text-indent:-.25in;mso-list:l0 level1 lfo1'><![if !supportLists]><span
  style='mso-fareast-font-family:Verdana;mso-bidi-font-family:Verdana'><span
  style='mso-list:Ignore'>-<span style='font:7.0pt "Times New Roman"'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  </span></span></span><![endif]><span style='mso-bidi-font-size:10.0pt;
  mso-bidi-font-family:Arial'>Continuous indexing of criteria's (zero based)</span><span
  style='mso-fareast-font-family:"Times New Roman"'><o:p></o:p></span></p>
  <p class=MsoListParagraphCxSpLast style='text-indent:-.25in;mso-list:l0 level1 lfo1'><![if !supportLists]><span
  style='mso-fareast-font-family:Verdana;mso-bidi-font-family:Verdana'><span
  style='mso-list:Ignore'>-<span style='font:7.0pt "Times New Roman"'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  </span></span></span><![endif]><span style='mso-fareast-font-family:"Times New Roman"'>“none”
  to utilize Fixed length raw string feature<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:14'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>pin<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>The
  pin to capture CTV from<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:15'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>ctv_index_range<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>The
  CTV bit range to capture for that criteria<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:16'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>condition
  <o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Specify
  the condition for acceptance criteria. <o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Multiple
  conditions are supported by “|” delimiter.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Expected
  format:<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'><span
  style='mso-spacerun:yes'>    </span>&lt;pin&gt;,&lt;bit
  range&gt;,&lt;expected value&gt;|&lt;pin&gt;,&lt;bit range&gt;,&lt;expected
  value&gt;<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Each
  condition is evaluated as follows:<o:p></o:p></span></p>
  <p class=MsoListParagraphCxSpFirst style='text-indent:-.25in;mso-list:l0 level1 lfo1'><![if !supportLists]><span
  style='mso-fareast-font-family:Verdana;mso-bidi-font-family:Verdana'><span
  style='mso-list:Ignore'>-<span style='font:7.0pt "Times New Roman"'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  </span></span></span><![endif]><span style='mso-fareast-font-family:"Times New Roman"'>If
  the value in the given &lt;bit range&gt; on the given &lt;pin&gt;,<br>
  match the given &lt;expected value&gt; </span><span style='font-family:Wingdings;
  mso-ascii-font-family:Verdana;mso-fareast-font-family:"Times New Roman";
  mso-hansi-font-family:Verdana;mso-char-type:symbol;mso-symbol-font-family:
  Wingdings'><span style='mso-char-type:symbol;mso-symbol-font-family:Wingdings'>è</span></span><span
  style='mso-fareast-font-family:"Times New Roman"'> condition pass<o:p></o:p></span></p>
  <p class=MsoListParagraphCxSpMiddle style='text-indent:-.25in;mso-list:l0 level1 lfo1'><![if !supportLists]><span
  style='mso-fareast-font-family:Verdana;mso-bidi-font-family:Verdana'><span
  style='mso-list:Ignore'>-<span style='font:7.0pt "Times New Roman"'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  </span></span></span><![endif]><span style='mso-fareast-font-family:"Times New Roman"'>Else
  </span><span style='font-family:Wingdings;mso-ascii-font-family:Verdana;
  mso-fareast-font-family:"Times New Roman";mso-hansi-font-family:Verdana;
  mso-char-type:symbol;mso-symbol-font-family:Wingdings'><span
  style='mso-char-type:symbol;mso-symbol-font-family:Wingdings'>è</span></span><span
  style='mso-fareast-font-family:"Times New Roman"'> condition fail<o:p></o:p></span></p>
  <p class=MsoListParagraphCxSpLast style='text-indent:-.25in;mso-list:l0 level1 lfo1'><![if !supportLists]><span
  style='mso-fareast-font-family:Verdana;mso-bidi-font-family:Verdana'><span
  style='mso-list:Ignore'>-<span style='font:7.0pt "Times New Roman"'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  </span></span></span><![endif]><span style='mso-fareast-font-family:"Times New Roman"'>If
  at least one condition fail, the entire condition will be evaluated as fail<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:17'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>hry_output_on_condition_fail<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>The
  output char for HRY raw string in case at least one condition fail.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Note
  that ‘<b style='mso-bidi-font-weight:normal'>9</b>’ is not a valid value as
  it is reserved for HRY fixed length feature.<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:18'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>bypass_global<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Specify
  the name (including collection) of integer type bypass global for flow
  control.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>If
  &lt;BypassGlobalPrefix&gt; is specified, it will be added to as prefix to the
  value in this attribute.<o:p></o:p></span></p>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Usage
  (during instance execution, i.e. in MAIN flow):<o:p></o:p></span></p>
  <p class=MsoListParagraphCxSpFirst style='text-indent:-.25in;mso-list:l0 level1 lfo1'><![if !supportLists]><span
  style='mso-fareast-font-family:Verdana;mso-bidi-font-family:Verdana'><span
  style='mso-list:Ignore'>-<span style='font:7.0pt "Times New Roman"'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  </span></span></span><![endif]><span style='mso-fareast-font-family:"Times New Roman"'>Prior
  to plist execution, global will be initialize as “1” (i.e. bypass)<o:p></o:p></span></p>
  <p class=MsoListParagraphCxSpLast style='text-indent:-.25in;mso-list:l0 level1 lfo1'><![if !supportLists]><span
  style='mso-fareast-font-family:Verdana;mso-bidi-font-family:Verdana'><span
  style='mso-list:Ignore'>-<span style='font:7.0pt "Times New Roman"'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  </span></span></span><![endif]><span style='mso-fareast-font-family:"Times New Roman"'>After
  plist execution, in case CTV data on relevant pin is found to be “1”, <br>
  global will be set to “-1” (i.e. execute or not-bypass)<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:19'>
  <td width=166 rowspan=5 style='width:124.65pt;border:inset teal 1.0pt;
  border-top:none;mso-border-top-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal align=center style='text-align:center'><span
  style='mso-fareast-font-family:"Times New Roman"'>&lt;Algorithms&gt;<o:p></o:p></span></p>
  </td>
  <td width=197 rowspan=4 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>&lt;Algorithm&gt;<o:p></o:p></span></p>
  </td>
  <td width=100 rowspan=4 style='width:74.75pt;border-top:none;border-left:
  none;border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Multiple
  times<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>index
  <o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Continuous
  indexing of algorithms (zero based)<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:20'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>name<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Algorithm
  name for ITUFF datalog purposes<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:21'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>pat_modify_label<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Optional<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Will
  be ignored if present. Just here for backwards compatibility with iCHSR config
  file.<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:22'>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>ctv_size<o:p></o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>The
  expected CTV data size for that specific algorithm (the number of captued vectors per pin).<o:p></o:p></span></p>
  </td>
 </tr>
 <tr style='mso-yfti-irow:23;mso-yfti-lastrow:yes'>
  <td width=197 style='width:2.05in;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'><o:p>&nbsp;</o:p></span></p>
  </td>
  <td width=100 style='width:74.75pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'>Only
  once<o:p></o:p></span></p>
  </td>
  <td width=212 style='width:158.7pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'><o:p>&nbsp;</o:p></span></p>
  </td>
  <td width=91 style='width:68.3pt;border-top:none;border-left:none;border-bottom:
  inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:inset teal .75pt;
  mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal style='mso-margin-top-alt:auto;mso-margin-bottom-alt:auto'><span
  style='mso-bidi-font-size:10.0pt;mso-bidi-font-family:Arial'>Required<o:p></o:p></span></p>
  </td>
  <td width=566 style='width:424.6pt;border-top:none;border-left:none;
  border-bottom:inset teal 1.0pt;border-right:inset teal 1.0pt;mso-border-top-alt:
  inset teal .75pt;mso-border-left-alt:inset teal .75pt;mso-border-alt:inset teal .75pt;
  padding:0in 5.4pt 0in 5.4pt'>
  <p class=MsoNormal><span style='mso-fareast-font-family:"Times New Roman"'><o:p>&nbsp;</o:p></span></p>
  </td>
 </tr>
</table>

### 3.1. Configuration File Changes from iCHSR   

Existing HRY mode .xml from iCHSR will continue to work. However there are some noteworthy additions/changes:  
**ctv_size** : To support different amounts of captures for different pins, the ctv_size can be a | deliminated list of "pin,size" (eg "Pin1,12|Pin2,6|...) where size is the number of captured vectors on that pin.     
   - The string "default" can be used as a pin name to match any pins not explicitly called out. (eg "PinA,12|default,30")  
   - A single decimal number can be used to apply the same size to all pins. (just like the current behavior eg "36")  
   - If only one algorithm is specified, and ReverseCtvCaptureData==N, then the ctv_size will be ignored. Instead it will only fail if not enough ctv are captured to do the required processing.  

**bypass globals** : Previously the bypass globals were forced to have the Collection name be the same as the module name. Now the module name (and IP scope) can be specified. The user vars are checked in the following order:   
   - bypass global as-is (BypassGlobalPrefix + Criteria.bypass_global)  
   - bypass global as-is + IP scope of test (IP of test + :: + BypassGlobalPrefix + Criteria.bypass_global)  
   - bypass global in IP scope of the test with module == collection. (IP of test + :: + collection + :: + BypassGlobalPrefix + Criteria.bypass_global)  
      - This is what the EVG templates does when you give it bypass_global=ARR_COMMON.MLC_DAT_S0_DR_DATA_FAIL_BOTH from an IP_CPU scoped test, it translates it to IP_CPU\::ARR_COMMON::ARR_COMMON.MLC_DAT_S0_DR_DATA_FAIL_BOTH.   
      - The problem with this is that ARR_COMMON is PKG scoped so something has to copy all the uservars from ARR_COMMON to IP_CPU::ARR_COMMON. This might not exist in Prime.  
   - bypass global in PKG scope with module == collection. (collection + :: + BypassGlobalPrefix + Criteria.bypass_global)  
      - Its up to the user to verify that the user var collection is marked as shared (normally IP scoped tests can't update PKG level user vars).  

**schema name/location** : The header information *xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="GEN_HSR_HRY.xsd"* is ignored. The schema is currently being loaded from ~USER_CODE_DLLS_PATH/ArrayHRY_XML.xsd  

**Full list of Schema changes:**  
   - Pin names in the condition field can contain lower case values.  
   - The passing value in the condition field is forced to be binary by the schema (previously the template checked after loading).  
   - Global names in bypass_global and \<BypassGlobalPrefix> can contain '::' (for IP/Module scoping).  
   - \<Algorithms> is now required by the schema (previously the template checked after loading).  
   - pat_modify_label field is optional. It never did anything, now it doesn't need to be specified.  
   - ctv_size field now accepts a single number or a | deliminated pin,size list -- "(\d+)|([A-Za-z0-9_:,]+[,][0-9]+([|][A-Za-z0-9_:]+,[0-9]+)*)"  
   - \<Algorithms> index and name must be unique. (previously it was being checked in the template)
   - The \<CtvToHryMapping> requires exactly 2 entries mapping ctv_data for both '0' and '1'.   

----  

### 3.2. Configuration File Examples  

#### 3.2.1 Configuration input file example    
```xml
<?xml version="1.0" encoding="utf-8"?>
<HSR_HRY_config xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="GEN_HSR_HRY.xsd">

<ReverseCtvCaptureData>N</ReverseCtvCaptureData>

<CtvToHryMapping>
	<Map ctv_data="0" hry_data="0" />
	<Map ctv_data="1" hry_data="1" />
</CtvToHryMapping>

<HryPrePostMapping>
	<Map hry_data="0" status="pass" />
	<Map hry_data="1" status="fail" />
	<Map hry_data="8" status="fail" />
	<PostRepairSymbol  symbol="R" />
</HryPrePostMapping>

<Criterias>
	<Criteria hry_index="0"  pin="P001" ctv_index_range="2"  condition="P002,0-1,00|P002,3,1"    hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="1"  pin="P001" ctv_index_range="6"  condition="P002,4-5,00|P002,7,1"    hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="2"  pin="P001" ctv_index_range="10" condition="P002,8-9,00|P002,11,1"   hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="3"  pin="P001" ctv_index_range="14" condition="P002,12-13,00|P002,15,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="4"  pin="P001" ctv_index_range="18" condition="P002,16-17,00|P002,19,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_2" />
	<Criteria hry_index="5"  pin="P001" ctv_index_range="22" condition="P002,20-21,00|P002,23,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_3" />
	<Criteria hry_index="6"  pin="P001" ctv_index_range="26" condition="P002,24-25,00|P002,27,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_3" />
	<Criteria hry_index="7"  pin="P001" ctv_index_range="30" condition="P002,28-29,00|P002,31,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_3" />
	<Criteria hry_index="8"  pin="P001" ctv_index_range="34" condition="P002,32-33,00|P002,35,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_4" />
</Criterias>

<Algorithms>
	<Algorithm index="0" name="SCAN"    pat_modify_label="" ctv_size="36" />
	<Algorithm index="1" name="PMOVI"   pat_modify_label="" ctv_size="36" />
	<Algorithm index="2" name="March-C" pat_modify_label="" ctv_size="36" />
</Algorithms>

</HSR_HRY_config>
```

#### 3.2.2 Configuration input file example with condition fail keys    
```xml
<?xml version="1.0" encoding="utf-8"?>
<HSR_HRY_config xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="GEN_HSR_HRY.xsd">

<ReverseCtvCaptureData>N</ReverseCtvCaptureData>

<CtvToHryMapping>
	<Map ctv_data="0" hry_data="0" />
	<Map ctv_data="1" hry_data="1" />
</CtvToHryMapping>

<ConditionFailKeys>
    <ConditionFailKey name="key1">
        <Map expected_data="01" hry_output="7"/>
        <Map expected_data="10" hry_output="6"/>
        <Map expected_data="11" hry_output="5"/>
    </ConditionFailKey>
    <ConditionFailKey name="key2">
        <Map expected_data="0" hry_output="4"/>
    </ConditionFailKey>
</ConditionFailKeys>

<Criterias>
	<Criteria hry_index="0"  pin="P001" ctv_index_range="2"  condition="P002,0-1,00,key1|P002,3,1,key2" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="1"  pin="P001" ctv_index_range="6"  condition="P002,4-5,00|P002,7,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="2"  pin="P001" ctv_index_range="10" condition="P002,8-9,00|P002,11,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="3"  pin="P001" ctv_index_range="14" condition="P002,12-13,00|P002,15,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="4"  pin="P001" ctv_index_range="18" condition="P002,16-17,00|P002,19,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_2" />
	<Criteria hry_index="5"  pin="P001" ctv_index_range="22" condition="P002,20-21,00|P002,23,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_3" />
	<Criteria hry_index="6"  pin="P001" ctv_index_range="26" condition="P002,24-25,00|P002,27,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_3" />
	<Criteria hry_index="7"  pin="P001" ctv_index_range="30" condition="P002,28-29,00|P002,31,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_3" />
	<Criteria hry_index="8"  pin="P001" ctv_index_range="34" condition="P002,32-33,00|P002,35,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_4" />
</Criterias>

<Algorithms>
	<Algorithm index="0" name="SCAN"    pat_modify_label="" ctv_size="36" />
	<Algorithm index="1" name="PMOVI"   pat_modify_label="" ctv_size="36" />
	<Algorithm index="2" name="March-C" pat_modify_label="" ctv_size="36" />
</Algorithms>

</HSR_HRY_config>
```

#### 3.2.3 Configuration input file example using fixed length feature   
```xml
<?xml version="1.0" encoding="utf-8"?>
<HSR_HRY_config xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="GEN_HSR_HRY.xsd">

<ReverseCtvCaptureData>N</ReverseCtvCaptureData>

<CtvToHryMapping>
	<Map ctv_data="0" hry_data="0" />
	<Map ctv_data="1" hry_data="1" />
</CtvToHryMapping>

<Criterias>
	<Criteria hry_index="0"  pin="P001" ctv_index_range="2"  condition="P002,0-1,00|P002,3,1"    hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="1"  pin="P001" ctv_index_range="none"  condition="P002,4-5,00|P002,7,1"    hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="2"  pin="P001" ctv_index_range="none" condition="P002,8-9,00|P002,11,1"   hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="3"  pin="P001" ctv_index_range="14" condition="P002,12-13,00|P002,15,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
	<Criteria hry_index="4"  pin="P001" ctv_index_range="18" condition="P002,16-17,00|P002,19,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_2" />
	<Criteria hry_index="5"  pin="P001" ctv_index_range="none" condition="P002,20-21,00|P002,23,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_3" />
	<Criteria hry_index="6"  pin="P001" ctv_index_range="26" condition="P002,24-25,00|P002,27,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_3" />
	<Criteria hry_index="7"  pin="P001" ctv_index_range="none" condition="P002,28-29,00|P002,31,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_3" />
	<Criteria hry_index="8"  pin="P001" ctv_index_range="34" condition="P002,32-33,00|P002,35,1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_4" />
</Criterias>

<Algorithms>
	<Algorithm index="0" name="SCAN"    pat_modify_label="" ctv_size="36" />
	<Algorithm index="1" name="PMOVI"   pat_modify_label="" ctv_size="36" />
	<Algorithm index="2" name="March-C" pat_modify_label="" ctv_size="36" />
</Algorithms>

</HSR_HRY_config>
```

#### 3.2.4 Configuration input file example with bypass global prefix   
```xml
<?xml version="1.0" encoding="utf-8"?>
<HSR_HRY_config xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="GEN_HSR_HRY.xsd">

<ReverseCtvCaptureData>N</ReverseCtvCaptureData>

<CtvToHryMapping>
	<Map ctv_data="0" hry_data="0" />
	<Map ctv_data="1" hry_data="1" />
</CtvToHryMapping>

<BypassGlobalPrefix>HSR.HRY_Global_</BypassGlobalPrefix>

<Criterias>
	<Criteria hry_index="0"  pin="P001" ctv_index_range="2"  condition="P002,0-1,00|P002,3,1"    hry_output_on_condition_fail="8" bypass_global="1" />
	<Criteria hry_index="1"  pin="P001" ctv_index_range="6"  condition="P002,4-5,00|P002,7,1"    hry_output_on_condition_fail="8" bypass_global="1" />
	<Criteria hry_index="2"  pin="P001" ctv_index_range="10" condition="P002,8-9,00|P002,11,1"   hry_output_on_condition_fail="8" bypass_global="1" />
	<Criteria hry_index="3"  pin="P001" ctv_index_range="14" condition="P002,12-13,00|P002,15,1" hry_output_on_condition_fail="8" bypass_global="1" />
	<Criteria hry_index="4"  pin="P001" ctv_index_range="18" condition="P002,16-17,00|P002,19,1" hry_output_on_condition_fail="8" bypass_global="2" />
	<Criteria hry_index="5"  pin="P001" ctv_index_range="22" condition="P002,20-21,00|P002,23,1" hry_output_on_condition_fail="8" bypass_global="3" />
	<Criteria hry_index="6"  pin="P001" ctv_index_range="26" condition="P002,24-25,00|P002,27,1" hry_output_on_condition_fail="8" bypass_global="3" />
	<Criteria hry_index="7"  pin="P001" ctv_index_range="30" condition="P002,28-29,00|P002,31,1" hry_output_on_condition_fail="8" bypass_global="3" />
	<Criteria hry_index="8"  pin="P001" ctv_index_range="34" condition="P002,32-33,00|P002,35,1" hry_output_on_condition_fail="8" bypass_global="4" />
</Criterias>

<Algorithms>
	<Algorithm index="0" name="SCAN"    pat_modify_label="" ctv_size="36" />
	<Algorithm index="1" name="PMOVI"   pat_modify_label="" ctv_size="36" />
	<Algorithm index="2" name="March-C" pat_modify_label="" ctv_size="36" />
</Algorithms>

</HSR_HRY_config>
```

#### 3.2.5 Configuration input file example multiple domains      
```xml
<?xml version="1.0" encoding="utf-8"?>
<HSR_HRY_config xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="GEN_HSR_HRY.xsd">

	<ReverseCtvCaptureData>N</ReverseCtvCaptureData>

	<CtvToHryMapping>
		<Map ctv_data="0" hry_data="0" />
		<Map ctv_data="1" hry_data="1" />
	</CtvToHryMapping>

	<Criterias>
		<Criteria hry_index="0"  pin="P001" ctv_index_range="0"     condition="P002,0-1,00"    hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
		<Criteria hry_index="1"  pin="P001" ctv_index_range="1"     condition="P002,2-3,00"    hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_2" />
	</Criterias>

	<Algorithms>
		<!-- 2 captures on P001, 4 captures on P002 ... Multi-algorithm so need per-pin ctv_size -->
		<Algorithm index="0" name="AlgA"  ctv_size="P001,2|P002,4" />      <!-- specify both pins --> 
		<Algorithm index="1" name="AlgB"  ctv_size="P001,2|default,4" />   <!-- specify one pin, "default" covers the others --> 
		<Algorithm index="2" name="AlgC"  ctv_size="default,2|P002,4" />   <!-- specify one pin, "default" covers the others --> 
	</Algorithms>

</HSR_HRY_config>
```


----  
## 4. Optional Features    

### 4.1. Raw String Forwarding Mode   
To support the ability of tracking changes in the HRY raw string, the template offers the ability to forward the raw string from one instance to another.
Following instance parameters should be set:  
   - _RawStringForwardingMode_ – this accepts 2 options: PRE, POST which indicates if the raw string should be stored (PRE) or retrieved (POST) to be compared with.
   - _SharedStorageKey_ – this is the shared storage key that will used in order to access the raw string (either store or retrieve)
If _SharedStorageKey_ is kept empty (the default value) the raw string forwarding feature is disabled.   

In addition to the instance parameters setup, configuration file must also include definition of \<HryPrePostMapping> element (see details and example above).  

**Usage**:  
When executing instance with _RawStringForwardingMode_ = PRE and some _SharedStorageKey_, the instance output of HRY raw string is stored in the given key. (the actual shared storage location is Context=DUT, Table=String, Key=_SharedStorageKey_ + "_" + _AlgorithmNameFromConfigFile_ --> unlike the ituff logging the "\_" is included even if the algorithm name is blank.)  
Later down the flow, when executing instance with _RawStringForwardingMode_ = POST and same _SharedStorageKey_, the raw string is retrieved from the key and compared to the current instance raw string.  
At this point, the code will use the \<HryPrePostMapping> definitions from the configuration file, to identify changes in the raw string (form PRE to POST) and if repair occurred (i.e. fail in PRE turn to pass in POST), the raw string will be marked, in the respective index, with the configuration file defined symbol specified by \<PostRepairSymbol>.  

**Example**:  

| Instance           | RawStringForwardingMode | SharedStorageKey | HRY Raw String |  
| :-----------             | :----------- | :----------- | :----------- |   
| 1st Instance    |  PRE        | MyKey       | 810101010    |
| 2nd Instance    |  POST       | MyKey       | 000100010    |
| Result (what the 2nd instance datalogs |  | | RR010R010 -- By the config file definitions, 1 & 8 indicates fail, while 0 indicates pass. So, any change of 1->0 or 8->0 indicates repair, hence marked as R.   |  

### 4.2. Raw string output per condition fail   
In the regular usage, when condition expected data does not match the captured data, the respective location in the HRY raw string is marked with the value given in ‘hry_output_on_condition_fail’ attribute, regardless of what the mismatch was (i.e. all mismatches get the same output in raw string).  
With the raw string output per condition fail, user have the ability to specify different output per expected data mismatch.  
To activate this feature, \<ConditionFailKeys> element must be specified in the configuration file.  

**Format example**:  
```xml
<ConditionFailKeys>
    <ConditionFailKey name="key1">
        <Map expected_data="01" hry_output="7"/>
        <Map expected_data="10" hry_output="6"/>
        <Map expected_data="11" hry_output="5"/>
    </ConditionFailKey>
</ConditionFailKeys>
```
With this, each expect data have its own unique HRY raw string output.
When \<ConditionFailKeys> element is defined, the \<Criteria> elements may use the keys defined by \<ConditionFailKey>, in the criteria ‘condition’ attribute following this format (to continue above example):
```xml
	<Criteria hry_index="0" pin="P001" ctv_index_range="2" condition="P002,0-1,00,key1" hry_output_on_condition_fail="8" bypass_global="HSR.HRY_Global_1" />
```
With this, in case the condition expected data (00) does not match the captured data on the pin (P002), the code will use the values specified in key1 to find a match and respected hry output to set in HRY raw string.
If yet no match is found, the value of ‘hry_output_on_condition_fail’ attribute will be used for the HRY raw string.

### 4.3. Fixed length raw string     
In the regular usage, each \<Criteria> element defined in the configuration file specifies an actual bit range in its ctv_index_range attribute (for example: ctv_index_range="2", ctv_index_range="3-6").   
The fixed length raw string feature supports the ability to avoid specifying actual bit range and instead set it to: “none” (i.e. ctv_index_range="none").  
When coming to datalog the HRY raw string, each \<Criteria> that specified ctv_index_range="none", will be marked by ‘9’ in their relative location in the raw string.   
The ‘9’ value is hard coded in the template and it is not user configurable.  
Due to this restriction, the following attributes in the config file may not specify ‘9’ as valid input:  

| Attribute | Element |  
| :----     | :----   |  
| hry_data  | CtvToHryMapping   |  
| hry_data  | HryPrePostMapping   |  
| symbol    | HryPrePostMapping   |  
| hry_output| ConditionFailKey   |  
| hry_output_on_condition_fail  | Criteria   |  


----  
## 5. TPL Samples  

```c-sharp
Test ArrayHRY PrimeArrayHRY_SamplePre_P2
{
   LevelsTc = "IP_CPU_BASE::DDR_univ_lvl_nom_lvl";
   Patlist = "arr_pbist_mclk_x_mcip_core_hry_ssa_mlc_dat_direct_prescreen_list";
   TimingsTc = "ARR_CORE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d50_SHARED";
   ConfigFile = "~MARRPBIST_PATMODIFY_PATH/GOLDEN_XML/arr_pbist_mclk_x_mcip_core_hry_ssa_mlc_dat_direct_prescreen_list.xml";
   RawStringForwardingMode = "PRE";
   SharedStorageKey = "PrimeArrayPrePostKey1";
   LogLevel = "PRIME_DEBUG";
}

Test ArrayHRY PrimeArrayHRY_SamplePost_P2
{
   LevelsTc = "IP_CPU_BASE::DDR_univ_lvl_nom_lvl";
   Patlist = "arr_pbist_mclk_x_mcip_core_hry_ssa_mlc_dat_direct_prescreen_list";
   TimingsTc = "ARR_CORE::cpu_func_sdr_univ_sta_univ_univ_b100_t100_d50_SHARED";
   ConfigFile = "~MARRPBIST_PATMODIFY_PATH/GOLDEN_XML/arr_pbist_mclk_x_mcip_core_hry_ssa_mlc_dat_direct_prescreen_list.xml";
   RawStringForwardingMode = "POST";
   SharedStorageKey = "PrimeArrayPrePostKey1";
   LogLevel = "PRIME_DEBUG";
}
```


###  

----  
## 6. ITUFF datalog   

The HRY raw string will be datalog to ITUFF in the following format:
(if the algorithm name is blank the preceding underscore is removed.)
```
2_tname_<module and instance name>_<algorithm name>_HRY_RAWSTR_<wrap counter>
2_strgval_<HRY raw string>
```

Example (with algorithm name == SCAN):
```
2_tname_MyModule::MyInstance_SCAN_HRY_RAWSTR_1
2_strgval_810101010
```

Example (no algorithm name):
```
2_tname_MyModule::MyInstance_HRY_RAWSTR_1
2_strgval_810101010
```

Note: for large raw string length (i.e. longer than 3900 characters), the datalog will be split to multiple lines where each tname will have the suffix incrementing the wrap counter.
Example:
```
2_tname_MyModule::MyInstance_SCAN_HRY_RAWSTR_1   (Wrap counter always starts with ‘1’).
2_strgval_810101010101011100010101110011…..
2_tname_MyModule::MyInstance_SCAN_HRY_RAWSTR_2
2_strgval_11111111010
```


###  

----  
## 7. Exit Ports  

| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | Any test class setup or logic failure (ex: config file parsing). |   
| 1   | Pass  | Pass Port. All CTV used in the criteria are 0 and all conditions pass. (HRY string is all 0's) |   
| 2   | Pass  | The CTV capture data (of at least one criteria) contains value other than zero. |   
| 3   | Pass  | If at least one condition (of at least one criteria) failed. Port 2 has higher priority. |   

###

----  
