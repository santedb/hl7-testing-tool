# HL7v2 Testing Tool 
This project is a C# console application leveraging the nHapi.Parser.Agfa package to build test suites for HL7v2 messaging. This console application has been created to configure and execute a test suite for HL7v2 messages being sent and received with the MLLP protocol.

# XML Configuration Files

There are naming conventions being used for elements and attributes of an XML file representing a test step or precondition to a test case. A test suite is represented by a directory within the project containing all test steps. These conventions are explained next 

## Directory/File Naming

The directory containing a test suite is named '**data**'and should not have its name altered to ensure test steps are read by this application to build a test suite. Test steps represented by an XML file are named using the convention '**OHIE-CR-#-#**', where the first number (#) represents a case number and the second number represents the test number for that specific case. Case numbers and step numbers are parsed from these file names to build the test suite accordingly.

## Elements
Recognized XML elements are used to create each test step.

|Element              |Description               |Example                         |
|------------|------------------------------------------------|-----------------------------|
|testStep    |Root element representing a *TestStep* object.  |`<testStep>...</testStep>`   |
|description |Description of the step as a string.            |`<description>...</description>`|
|message     |HL7v2 message sent to illicit an HL7v2 response.|`<message>...</message>`|
|assertions  |List of assertions for an HL7v2 response.       |`<assertions>...</assertions>`|
|assert      |Uses a terser string to parse HL7v2 response segments, fields, components, or sub-components to assert a value.                              |`<assert terserString="MSH-12" value="2.3.1" />`|

## Attributes
Assertions can be mandatory, missing, not missing, or alternate (multiple possible assertions for a single terser). Additional attributes are used with the `<assert/>` element for the various assertions. 

|Attribute |Description                    |
|----------|-------------------------------|
|missing   |Asserts a missing segment, field, component, sub-component when `missing="true"`. Asserts not missing when `missing="false"`.|
|alternate |Asserts an alternate value for a specific terser string in case alternate values are expected whenever `alternate="true"` for and `terserString` attributes have the same value for multiple `<assert/>` elements.|

## Sample File: OHIE-CR-03-20.xml
```markdown
<testStep>
	<description>Test harness sends ADT^A01 message having invalid assigning authority name in CX.4.1</description>
	<message>MSH|^~\&amp;|TEST_HARNESS^^|TEST^^|CR1^^|MOH_CAAT^^|20141104174451|TEST_HARNESS+TEST_HARNESS|ADT^A01^ADT_A01|TEST-CR-03-20|P|2.3.1
EVN||20101020
PID|||RJ-999-2^^^TEST_BLOCK||THAMES^ROBERT^^^^^L| |1983|M|||1220 Centennial Farm Road^^ELLIOTT^IA^51532||^PRN^PH^^^712^7670867||||||481-27-4185
PV1||I</message>
	<assertions>
		<assert terserString="MSA-1" value="AE" alternate="true" />
		<assert terserString="MSA-1" value="CE" alternate="true" />
		<assert terserString="MSA-1" value="AR" alternate="true" />
		<assert terserString="MSA-1" value="CR" alternate="true" />
		<assert terserString="MSH-5" value="TEST_HARNESS" />
		<assert terserString="MSH-6" value="TEST" />
		<assert terserString="ERR-1-4-2" value="Error processig assigning authority" />
		<assert terserString="MSH-9-1" value="ACK" />
		<assert terserString="MSH-9-2" value="A01" />
        <assert terserString="MSH-12" value="2.3.1" />
        	
	</assertions>
</testStep>
```
