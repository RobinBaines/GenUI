The GenUI executable is used to generate classes for the views defined in the dataset in a visual studio project.
For example:

run\genui BAINESLENOVO TestDb c:\Projects\AppsLib\libbaines\TestApp\TestApp\ c:\projects\GenUI\OUTPUT\

Looks for tables and views in Dataset xsd files in a project folder: c:\Projects\AppsLib\libbaines\TestApp\TestApp\
Looks up the definition of the views on SQL Server BAINESLENOVO in database TestDb and generates code (uses the SQL Server api calls).
This only works if a single table or view is used in a tableadapter in the dataset. 

For example the above generates code gen_TheDataSet_v_usr_log.vb

Public Class TheDataSet_v_usr_log
Inherits Utilities.dgColumns
'columns of the datagrid.
'textboxes for the filter textboxes above each column.

