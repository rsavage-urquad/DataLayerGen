# DataLayerGen

This C# project is a Code Generator to create MS SQL Stored Procedures, C# Model and Data Layer for a Table. The results are not intended to be a 100% solution, just a starting point.

## Templates

| Template | Purpose |
| --- | :--- |
| Get By Id | Stored Procedure to retrieve a row for the requested Id. |
| Get By Name | Stored Procedure to retrieve a row for the requested Name. |
| Insert | Stored Procedure to insert a row. |
| Update | Stored Procedure to update a row. |
| Delete | Stored Procedure to delete a row. |
| Toggle Active | Stored Procedure to toggle the active indicator for a requested Id. |
| C# Entity Class | POCO Class for an Entity. |
| C# Data Layer Class | C# Class to perform SQL CRUD operations for the Entity. |

## Template Commands

Don't like my output format?  Feel free to build your own templates.  This section will contain details on the commands that can be used within a Template.

### Commands

The following is a list of the commands that can be used in a template.

* **`{{Date}}`** - Inserts the current date (M/d/yyyy format).
* **`{{Schema}}`** - Inserts the selected table's schema name.
* **`{{Table}}`** - Inserts the selected table's name.
* **`{{CamelTable}}`** - Inserts the selected table's name in camel case.
* **`{{Each|<Collection>|<Format>}}`** - Will repeat the `"<Format>"` result for each item in the `<Collection>` (*more details in a later section*).
* **`{{If|<Variable>|<True Format>|<False Format>}}`** - Will insert the `<True Format>` if the `<Variable>` is true.  Otherwise the `<False Format>` is inserted  (*more details in a later section*).
* **`{{SectionIf|<Variable>}}...{{/SectionIf}}`** - Placed on separate lines with conditional content in between.  If the condition `<Variable>` is true, the content within the tags will be included (*more details in a later section*).  At this time, embedded **`{{SectionIf|<Variable>}}...{{/SectionIf}}`** are not supported.
* **`{{NameColName}}`** - Inserts the "Name" column's name.
* **`{{CamelNameColName}}`** - Inserts the "Name" column's name in camel case.
* **`{{NameColType}}`** - Inserts the "Name" column's SQL Data Type.
* **`{{NameColCodeType}}`** - Inserts the "Name" column's Code Data Type.
* **`{{ActiveCol}}`** - Inserts the "Active" column's name.
* **`{{ActiveColType}}`** - Inserts the "Active" column's SQL Data Type.
* **`{{ActiveColCodeType}}`** - Inserts the "Active" column's Code Data Type.
* **`{{ActiveValue}}`** - Inserts the "Active" column's Active value.
* **`{{InactiveValue}}`** - Inserts the "Active" column's Inactive value.
* **`{{CamelIdColParameters}}`** - Id column(s) as parameters to a method (i.e. - "int tableId, ...").

### Details for `{{Each|<Collection>|<Format>}}`

#### Collections

* **`ColList`** - All columns in the selected table.
* **`IdCols`** - All columns identified as the primary Id for the selected table.
* **`ColListExceptId`** - All columns in table except Id column(s).

#### Variables that can be used within the `{{Each ...}` `<format>`

* **`[[ColName]]`** - Column Name
* **`[[CamelColName]]`** - Column Name in camel case format (i.e. - first character lowercase).
* **`[[ColSqlType]]`** - Column's SQL Data Type (i.e. - varchar(50), money, ...).
* **`[[ColCodeType]]`** - Column's C# Data Type (i.e. - string, decimal ...).

### `{{If}|...}` and `{{SectionIf|...}}` Variables

* **`ActivePresent`** - True if the selected "ActiveCol" is not equal blank.
* **`ActiveIsString`** - True if the entered "Active" value is a string (or character).
* **`ActiveIsNotString`** - True if the entered "Active" value is not a string (or character).
* **`IdIsIdentity`** - True if the "Is Identity Column?" item is checked.
* **`IdIsNotIdentity`** - True if the "Is Identity Column?" item is not checked.

### Modifiers

Used in the `<Format>` section of an `{{Each ...}}` command, modifiers can be added to include text for specific iterations.

* **`[First|<True Format>:<False Format>]`** - Will insert the `<True Format>` for the first item in a collection, otherwise the `<False Format>` will be inserted.  Either format can be blank.
* **`[Last|<True Format>:<False Format>]`** - Will insert the `<True Format>` for the last item in a collection, otherwise the `<False Format>` will be inserted.  Either format can be blank.

### Examples

* `{{Each|ColList|[[ColName]][Last|:,]}}` would list all Column Names followed by a "," for all columns except the last one.
* `{{If|ActivePresent|@IncludeInactive bit = 0}}` - Includes the text `@IncludeInactive bit = 0` if the "ActiveCol" selection is **not** blank.
