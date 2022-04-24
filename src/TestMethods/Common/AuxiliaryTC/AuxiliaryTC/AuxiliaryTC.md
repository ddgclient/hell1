# AuxiliaryTC Template

### Rev 1/10/2022 (fmurillo)

| Contents      |    
| :----------- |  
| 1. [Introduction](#introduction)      |   
| 2. [Test Instance Parameters](#test-instance-parameters)   |   
| 3. [Expression Engine](#expression-engine)   |   
| 4. [TPL Samples](#tpl-samples)   |   
| 5. [Exit Ports](#exit-ports)   |  

###

----  
## 1. Introduction  
AuxiliaryTC is a basic utility test method that allows user to evaluate expressions using SharedStorage, Uservars and/or DFF and store evaluate result into a new token, and/or assign an exit port based on a second expression.

----  
## 2. Test Instance Parameters  

| Parameter Name       | Required? | Type | Values |  Comments |  
| :-----------         | :----------- | :----------- | :----------- | :----------- |   
| Expression           | Yes | String |  | Expression to be evaluated. Uses NCalc third party as engine. Tokens should use '[]'. |  
| DataType             | Yes | Enum   |  | String, Double or Integer.|  
| Storage              | No  | Enum   |  | Defines storage type for ResultToken. SharedStorage, Uservar or DFF.|  
| ResultToken          | No  | String |  | Token name where result will be stored. Uses Storage and DataType to define the variable to use but shared storge should still be the full format G.U.S.blah.|  
| ResultPort           | No  | String |  | Expression to determine exit port. Expected to be solved as int. Uses [R] as result token.|  
| Datalog              | No  | Enum   |  | Enabled or Disabled. Integer and string will be printed as strgval while double will be set as msrlt.|  

----  
## 3. Expression Engine
This test method supports Ncalc as expression engine. https://github.com/ncalc/ncalc <br>
It is able to evaluate simple mathematical expression using SharedStorage, Uservar and/or DFF tokens.<br>

All tokens must be specified using '[]'. Value will be evaluated in the following order:
- SharedStorage: Should be of the form G._context_._type_.Token where _context_ is L for Lot, U for DUT/Unit or I for IP, and _type_ is S for string, D for double or I for integer.  
- UserVar: String, Double and Integer. Always using full _collection_._name_ format with the full IP/Module scoping on the collection.  
- DFF: Using token name. It will be read from the current optype and die id.  

#### Supported functions:
https://github.com/ncalc/ncalc/wiki/Functions

#### Additional functions:   
| Function | Details |   
| :-----   | :-----  |   
| ToInt32(*value*)   | Converts a string or double value to an integer |   
| ToDouble(*value*)  | Converts a string or int value to a double      |   
| Random()         | Generates a random value between 0 and 1        |   
| Substring(*value*, *start*, *length*) | Creates a substring of *value*, starting at index *start* (0 is first bit), and containing *length* bits |   
| Bin2Dec(*value*  |  Converts the binary string *value* into an integer. *value* must be less than 32 bits.  |   
| Dec2Bin(*value*, *bits*)  | Converts the integer *value* into a binary string containing *bits* number of bits. MSB first, no radix, just a raw binary string: Dec2Bin(5, 6) -> 000101  |   
| Reverse(*value*)  | Reverses the string *value*   |   


----  
## 4. TPL Samples  

###  

```perl
Test AuxiliaryTC InstanceName  
{
    Expression = "[Token1]+ToInt32[Token2]";  
    DataType = "Integer";  
    Storage = "SharedStorage";  
    ResultToken = "ResultToken";  
    ResultPort = "[R]>0?2:1";  
}  

```


----  
## 4. Exit Ports  

| Exit Port       | Condition   | Description |   
| -----------     | ----------- | ----------- |    
| -2  | Alarm | Any HW alarm condition |    
| -1  | Error | Any software error |   
| 0   | Fail  | nothing causes this today. |   
| 1-20| Pass  | User assigned. |   

###

----  
