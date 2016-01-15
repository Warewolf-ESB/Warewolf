using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using TechTalk.SpecFlow;
using Warewolf.Studio.UISpecs.OutsideWorkflowDesignSurfaceUIMapClasses;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public class ActionSteps
    {
        [Given(@"The '(.*)' recorded action is performed")]
        [When(@"The '(.*)' recorded action is performed")]
        [Then(@"The '(.*)' recorded action is performed")]
        public void ThenTheRecordedActionIsPerformed(string p0)
        {
            switch (p0)
            {
                case "Assert_NewWorkFlow_RibbonButton_Exists":
                    {
                        Uimap.Assert_NewWorkFlow_RibbonButton_Exists();
                        break;
                    }
                case "Click_New_Workflow_Ribbon_Button":
                    {
                        Uimap.Click_New_Workflow_Ribbon_Button();
                        break;
                    }
                case "Assert_WebSource_ReqTypeComboBox_Exists":
                    {
                        Uimap.Assert_WebSource_ReqTypeComboBox_Exists();
                        break;
                    }
                case "Click_Settings_Ribbon_Button":
                    {
                        Uimap.Click_Settings_Ribbon_Button();
                        break;
                    }
                case "Click_New_DB_Connector_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_New_DB_Connector_Ribbon_Button();
                        break;
                    }
                case "Select_Data_Source_Droplist":
                    {
                        Uimap.Select_Data_Source_Droplist();
                        break;
                    }
                case "Select_Action_Droplist":
                    {
                        Uimap.Select_Action_Droplist();
                        break;
                    }
                case "Test_Connector_Calculate_Outputs":
                    {
                        Uimap.Test_Connector_Calculate_Outputs();
                        break;
                    }
                case "Click_New_Plugin_Connector_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_New_Plugin_Connector_Ribbon_Button();
                        break;
                    }
                case "Select_Plugin_Connector_Source_Droplist":
                    {
                        Uimap.Select_Plugin_Connector_Source_Droplist();
                        break;
                    }
                case "Select_Plugin_Connector_Namespace_Droplist":
                    {
                        Uimap.Select_Plugin_Connector_Namespace_Droplist();
                        break;
                    }
                case "Select_Plugin_Connector_Action_Droplist":
                    {
                        Uimap.Select_Plugin_Connector_Action_Droplist();
                        break;
                    }
                case "Plugin_Connectot_Test_Connector_Calculate_Outputs":
                    {
                        Uimap.Plugin_Connectot_Test_Connector_Calculate_Outputs();
                        break;
                    }
                case "Assert_StartNode_Exists":
                    {
                        Uimap.Assert_StartNode_Exists();
                        break;
                    }
                case "Assert_MultiAssign_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_MultiAssign_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Base_Conversion_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Base_Conversion_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Case_Conversion_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Case_Conversion_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Data_Merge_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Data_Merge_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Data_Split_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Data_Split_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Decision_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Decision_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Find_Index_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Find_Index_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Replace_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Replace_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sequence_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sequence_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Switch_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Switch_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Copy_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Copy_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Create_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Create_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Delete_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Delete_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Read_File_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Read_File_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Read_Folder_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Read_Folder_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Rename_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Rename_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Unzip_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Unzip_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Write_File_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Write_File_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Zip_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Zip_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_For_Each_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_For_Each_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Format_Number_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Format_Number_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Length_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Length_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Random_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Random_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Script_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Script_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sharepoint_Create_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sharepoint_Create_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sharepoint_Delete_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sharepoint_Delete_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sharepoint_Read_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sharepoint_Read_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sharepoint_Update_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sharepoint_Update_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sort_Records_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sort_Records_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sql_Bulk_insert_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sql_Bulk_insert_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_System_information_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_System_information_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Unique_Records_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Unique_Records_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Web_Request_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Web_Request_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_XPath_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_XPath_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Calculate_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Calculate_Exists_OnDesignSurface();
                        break;
                    }
                case "Drag_Toolbox_XPath_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_XPath_Onto_DesignSurface();
                        break;
                    }
                case "Assert_CMD_Line_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_CMD_Line_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Comment_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Comment_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Count_Records_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Count_Records_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Create_JSON_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Create_JSON_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Delete_Record_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Delete_Record_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Move_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Move_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Date_And_Time_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Date_And_Time_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_DateTime_Difference_Conversion_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_DateTime_Difference_Conversion_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Email_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Email_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Dropbox_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Dropbox_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Find_Record_Index_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Find_Record_Index_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_VariableList_DeleteButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_DeleteButton_Exists();
                        break;
                    }
                case "Assert_VariableList_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_Exists();
                        break;
                    }
                case "Assert_VariableList_Recordset_ChildTextBox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_Recordset_ChildTextBox_Exists();
                        break;
                    }
                case "Assert_VariableList_RecordsetInput_CheckBox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_RecordsetInput_CheckBox_Exists();
                        break;
                    }
                case "Assert_VariableList_RecordsetInput_ChildCheckBox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_RecordsetInput_ChildCheckBox_Exists();
                        break;
                    }
                case "Assert_VariableList_RecordsetItem_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_RecordsetItem_Exists();
                        break;
                    }
                case "Assert_VariableList_RecordsetOutput_CheckBox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_RecordsetOutput_CheckBox_Exists();
                        break;
                    }
                case "Assert_VariableList_RecordsetOutput_ChildCheckBox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_RecordsetOutput_ChildCheckBox_Exists();
                        break;
                    }
                case "Assert_VariableList_RecordsetTextBox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_RecordsetTextBox_Exists();
                        break;
                    }
                case "Assert_VariableList_SortButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_SortButton_Exists();
                        break;
                    }
                case "Assert_VariableList_VariableInput_CheckBox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_VariableInput_CheckBox_Exists();
                        break;
                    }
                case "Drag_Toolbox_MultiAssign_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Base_Conversion_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Base_Conversion_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Calculate_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Calculate_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Case_Conversion_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Case_Conversion_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_CMD_Line_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_CMD_Line_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Comment_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Comment_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Copy_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Copy_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Count_Records_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Count_Records_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Create_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Create_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Data_Merge_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Data_Merge_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Data_Picker_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Data_Picker_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Data_Split_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Data_Split_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Date_And_Time_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Date_And_Time_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_DateTime_Difference_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_DateTime_Difference_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Decision_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Decision_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Delete_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Delete_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Delete_Record_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Delete_Record_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Dropbox_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Dropbox_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Email_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Email_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Find_Index_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Find_Index_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Find_Record_Index_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Find_Record_Index_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_For_Each_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_For_Each_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Format_Number_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Format_Number_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_JSON_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_JSON_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Length_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Length_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Move_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Move_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Read_File_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Read_File_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Read_Folder_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Read_Folder_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Rename_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Rename_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Replace_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Replace_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Script_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Script_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Sequence_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Sequence_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Service_Picker_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Service_Picker_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Sharepoint_Delete_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Sharepoint_Delete_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Sharepoint_Read_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Sharepoint_Read_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Sort_Record_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Sort_Record_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Switch_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Switch_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_System_Information_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_System_Information_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Unique_Records_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Unique_Records_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Unzip_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Unzip_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_Web_Request_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Web_Request_Onto_DesignSurface();
                        break;
                    }
                case "Assert_VariableList_VariableItem_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_VariableItem_Exists();
                        break;
                    }
                case "Drag_Toolbox_Write_File_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Write_File_Onto_DesignSurface();
                        break;
                    }
                case "Assert_VariableList_VariableOutput_CheckBox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_VariableOutput_CheckBox_Exists();
                        break;
                    }
                case "Assert_VariableList_VariableTextBox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_VariableTextBox_Exists();
                        break;
                    }
                case "Drag_Toolbox_Zip_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Zip_Onto_DesignSurface();
                        break;
                    }
                case "Assert_VariableList_DataInputTree_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_VariableList_DataInputTree_Exists();
                        break;
                    }
                case "Drag_Toolbox_Random_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_Random_Onto_DesignSurface();
                        break;
                    }
                case "Open_Assign_Tool_Large_View":
                    {
                        Uimap.Open_Assign_Tool_Large_View();
                        break;
                    }
                case "Open_Base_Conversion_Tool_Qvi_Large_View":
                    {
                        Uimap.Open_Base_Conversion_Tool_Qvi_Large_View();
                        break;
                    }
                case "Open_Case_Conversion_Tool_Qvi_Large_View":
                    {
                        Uimap.Open_Case_Conversion_Tool_Qvi_Large_View();
                        break;
                    }
                case "Open_CMD_Line_Tool_Large_View":
                    {
                        Uimap.Open_CMD_Line_Tool_Large_View();
                        break;
                    }
                case "Open_Copy_Tool_Large_View":
                    {
                        Uimap.Open_Copy_Tool_Large_View();
                        break;
                    }
                case "Open_Create_Tool_Large_View":
                    {
                        Uimap.Open_Create_Tool_Large_View();
                        break;
                    }
                case "Open_Data_Merge_Large_View":
                    {
                        Uimap.Open_Data_Merge_Large_View();
                        break;
                    }
                case "Open_Data_Split_Large_View":
                    {
                        Uimap.Open_Data_Split_Large_View();
                        break;
                    }
                case "Open_Data_Split_Tool_Qvi_Large_View":
                    {
                        Uimap.Open_Data_Split_Tool_Qvi_Large_View();
                        break;
                    }
                case "Open_Email_Tool_Large_View":
                    {
                        Uimap.Open_Email_Tool_Large_View();
                        break;
                    }
                case "Open_Find_Record_Index_Tool_Large_View":
                    {
                        Uimap.Open_Find_Record_Index_Tool_Large_View();
                        break;
                    }
                case "Open_Json_Tool_Large_View":
                    {
                        Uimap.Open_Json_Tool_Large_View();
                        break;
                    }
                case "Open_Json_Tool_Qvi_Large_View":
                    {
                        Uimap.Open_Json_Tool_Qvi_Large_View();
                        break;
                    }
                case "Open_Move_Tool_Large_View":
                    {
                        Uimap.Open_Move_Tool_Large_View();
                        break;
                    }
                case "Open_Read_File_Tool_Large_View":
                    {
                        Uimap.Open_Read_File_Tool_Large_View();
                        break;
                    }
                case "Open_Read_Folder_Tool_Large_View":
                    {
                        Uimap.Open_Read_Folder_Tool_Large_View();
                        break;
                    }
                case "Open_Rename_Tool_Large_View":
                    {
                        Uimap.Open_Rename_Tool_Large_View();
                        break;
                    }
                case "Open_Sharepoint_Create_Tool_Large_View":
                    {
                        Uimap.Open_Sharepoint_Create_Tool_Large_View();
                        break;
                    }
                case "Open_Sharepoint_Delete_Tool_Large_View":
                    {
                        Uimap.Open_Sharepoint_Delete_Tool_Large_View();
                        break;
                    }
                case "Open_Sharepoint_Read_Tool_Large_View":
                    {
                        Uimap.Open_Sharepoint_Read_Tool_Large_View();
                        break;
                    }
                case "Open_Sharepoint_Update_Tool_Large_View":
                    {
                        Uimap.Open_Sharepoint_Update_Tool_Large_View();
                        break;
                    }
                case "Open_SQL_Bulk_Insert_Tool_Large_View":
                    {
                        Uimap.Open_SQL_Bulk_Insert_Tool_Large_View();
                        break;
                    }
                case "Open_SQL_Bulk_Insert_Tool_Qvi_Large_View":
                    {
                        Uimap.Open_SQL_Bulk_Insert_Tool_Qvi_Large_View();
                        break;
                    }
                case "Open_System_Information_Tool_Qvi_Large_View":
                    {
                        Uimap.Open_System_Information_Tool_Qvi_Large_View();
                        break;
                    }
                case "Open_Unzip_Tool_Large_View":
                    {
                        Uimap.Open_Unzip_Tool_Large_View();
                        break;
                    }
                case "Open_Web_Request_Tool_Large_View":
                    {
                        Uimap.Open_Web_Request_Tool_Large_View();
                        break;
                    }
                case "Open_Write_File_Tool_Large_View":
                    {
                        Uimap.Open_Write_File_Tool_Large_View();
                        break;
                    }
                case "Open_Xpath_Tool_Large_View":
                    {
                        Uimap.Open_Xpath_Tool_Large_View();
                        break;
                    }
                case "Open_Xpath_Tool_Qvi_Large_View":
                    {
                        Uimap.Open_Xpath_Tool_Qvi_Large_View();
                        break;
                    }
                case "Open_Zip_Tool_Large_View":
                    {
                        Uimap.Open_Zip_Tool_Large_View();
                        break;
                    }
                case "Assert_Assign_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Assign_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Assign_QVI_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Assign_QVI_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Base_Conversion_Qvi_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Base_Conversion_Qvi_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Case_Conversion_Qvi_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Case_Conversion_Qvi_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_CMD_Line_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_CMD_Line_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Copy_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Copy_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Create_Path_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Create_Path_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Data_Merge_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Data_Merge_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Data_Merge_Qvi_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Data_Merge_Qvi_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Data_Split_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Data_Split_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Data_Split_Qvi_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Data_Split_Qvi_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Email_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Email_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Find_Record_index_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Find_Record_index_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Json_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Json_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Json_Qvi_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Json_Qvi_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Move_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Move_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Read_File_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Read_File_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Read_Folder_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Read_Folder_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Rename_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Rename_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sharepoint_Create_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sharepoint_Create_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sharepoint_Delete_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sharepoint_Delete_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sharepoint_Read_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sharepoint_Read_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sharepoint_Update_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sharepoint_Update_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_SQL_Bulk_Insert_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_SQL_Bulk_Insert_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Sql_Bulk_insert_Qvi_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Sql_Bulk_insert_Qvi_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_System_Info_Qvi_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_System_Info_Qvi_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Unzip_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Unzip_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Web_Request_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Web_Request_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Write_File_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Write_File_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Xpath_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Xpath_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Xpath_Qvi_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Xpath_Qvi_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Zip_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Zip_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Open_Assign_Tool_Qvi_Large_View":
                    {
                        Uimap.Open_Assign_Tool_Qvi_Large_View();
                        break;
                    }
                case "Open_Data_Merge_Tool_Qvi_Large_View":
                    {
                        Uimap.Open_Data_Merge_Tool_Qvi_Large_View();
                        break;
                    }
                case "DebugOutput_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.DebugOutput_Exists();
                        break;
                    }
                case "DebugOutput_ExpandCollapseButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.DebugOutput_ExpandCollapseButton_Exists();
                        break;
                    }
                case "DebugOutput_FilterTextbox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.DebugOutput_FilterTextbox_Exists();
                        break;
                    }
                case "DebugOutput_ResultsTree_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.DebugOutput_ResultsTree_Exists();
                        break;
                    }
                case "DebugOutput_SettingsButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.DebugOutput_SettingsButton_Exists();
                        break;
                    }
                case "Assert_Toolbox_RefreshButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Toolbox_RefreshButton_Exists();
                        break;
                    }
                case "Assert_Toolbox_FilterTextbox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Toolbox_FilterTextbox_Exists();
                        break;
                    }
                case "Assert_Explorer_Edit_Connect_Control_Button_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Explorer_Edit_Connect_Control_Button_Exists();
                        break;
                    }
                case "Assert_Connect_Control_Exists_InExplorer":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Connect_Control_Exists_InExplorer();
                        break;
                    }
                case "Assert_Connect_ConnectControl_Button_Exists_InExplorer":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Connect_ConnectControl_Button_Exists_InExplorer();
                        break;
                    }
                case "Assert_Save_Button_Exists_OnDesignSurface":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Save_Button_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_New_Version_Download_Button_Exists_OnDesignSurface":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_New_Version_Download_Button_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Settings_Button_Exists_OnDesignSurface":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Settings_Button_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Debug_Button_Exists_OnDesignSurface":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Debug_Button_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Scheduler_Button_Exists_OnDesignSurface":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_Button_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Deploy_Button_Exists_OnDesignSurface":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Deploy_Button_Exists_OnDesignSurface();
                        break;
                    }
                 case "Assert_Knowledge_Base_Exists_OnDesignSurface":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Knowledge_Base_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_Lock_Button_Exists_OnDesignSurface":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Lock_Button_Exists_OnDesignSurface();
                        break;
                    }
                case "Assert_SaveDialog_CancelButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_CancelButton_Exists();
                        break;
                    }
                case "Assert_SaveDialog_ErrorLabel_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_ErrorLabel_Exists();
                        break;
                    }
                case "Assert_SaveDialog_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_Exists();
                        break;
                    }
                case "Assert_SaveDialog_ExplorerTree_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_ExplorerTree_Exists();
                        break;
                    }
                case "Assert_SaveDialog_ExplorerTreeItem_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_ExplorerTreeItem_Exists();
                        break;
                    }
                case "Assert_SaveDialog_ExplorerView_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_ExplorerView_Exists();
                        break;
                    }
                case "Assert_SaveDialog_FilterTextbox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_FilterTextbox_Exists();
                        break;
                    }
                case "Assert_SaveDialog_NameLabel_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_NameLabel_Exists();
                        break;
                    }
                case "Assert_SaveDialog_RefreshButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_RefreshButton_Exists();
                        break;
                    }
                case "Assert_SaveDialog_SaveButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_SaveButton_Exists();
                        break;
                    }
                case "Assert_SaveDialog_ServiceName_Textbox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_ServiceName_Textbox_Exists();
                        break;
                    }
                case "Assert_SaveDialog_WorkspaceName_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SaveDialog_WorkspaceName_Exists();
                        break;
                    }
                case "Assert_Database_Source_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Database_Source_Exists();
                        break;
                    }
                case "Assert_Plugin_Source_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Plugin_Source_Exists();
                        break;
                    }
                case "Assert_Web_Source_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Web_Source_Exists();
                        break;
                    }
                case "Click_Save_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Save_Ribbon_Button();
                        break;
                    }
                case "Click_Debug_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Debug_Ribbon_Button();
                        break;
                    }
                case "Click_Scheduler_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Scheduler_Ribbon_Button();
                        break;
                    }
                case "Click_Deploy_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Deploy_Ribbon_Button();
                        break;
                    }
                case "Click_Knowledge_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Knowledge_Ribbon_Button();
                        break;
                    }
                case "Click_Unlock_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Unlock_Ribbon_Button();
                        break;
                    }
                case "Click_NewDatabaseSource_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_NewDatabaseSource_Ribbon_Button();
                        break;
                    }
                case "Click_NewPluginSource_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_NewPluginSource_Ribbon_Button();
                        break;
                    }
                case "Click_NewWebSource_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_NewWebSource_Ribbon_Button();
                        break;
                    }
                case "Drag_Database_Connector_Onto_DesignSurface":
                    {
                        Uimap.Drag_Database_Connector_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Plugin_Connector_Onto_DesignSurface":
                    {
                        Uimap.Drag_Plugin_Connector_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Web_Connector_Onto_DesignSurface":
                    {
                        Uimap.Drag_Web_Connector_Onto_DesignSurface();
                        break;
                    }
                case "Move_Resource_Into_Folder":
                    {
                        Uimap.Move_Resource_Into_Folder();
                        break;
                    }
                case "Drag_Workflow_Onto_DesignSurface":
                    {
                        Uimap.Drag_Workflow_Onto_DesignSurface();
                        break;
                    }
                case "Assert_Scheduler_ConncectControl_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_ConncectControl_Exists();
                        break;
                    }
                case "Assert_Scheduler_ConnectButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_ConnectButton_Exists();
                        break;
                    }
                case "Assert_Scheduler_ConnectControl_Edit_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_ConnectControl_Edit_Exists();
                        break;
                    }
                case "Assert_Scheduler_CreateNewTask_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_CreateNewTask_Exists();
                        break;
                    }
                case "Assert_Scheduler_DeleteButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_DeleteButton_Exists();
                        break;
                    }
                case "Assert_Scheduler_DisabledRadioButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_DisabledRadioButton_Exists();
                        break;
                    }
                case "Assert_Scheduler_EditTrigger_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_EditTrigger_Exists();
                        break;
                    }
                case "Assert_Scheduler_EnabledRadioButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_EnabledRadioButton_Exists();
                        break;
                    }
                case "Assert_Scheduler_ErrorMessage_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_ErrorMessage_Exists();
                        break;
                    }
                case "Assert_Scheduler_HistoryInput_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_HistoryInput_Exists();
                        break;
                    }
                case "Assert_Scheduler_HistoryLabel_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_HistoryLabel_Exists();
                        break;
                    }
                case "Assert_Scheduler_HistoryTable_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_HistoryTable_Exists();
                        break;
                    }
                case "Assert_Scheduler_NameInput_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_NameInput_Exists();
                        break;
                    }
                case "Assert_Scheduler_NameLabel_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_NameLabel_Exists();
                        break;
                    }
                case "Assert_Scheduler_PasswordInput_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_PasswordInput_Exists();
                        break;
                    }
                case "Assert_Scheduler_PasswordLabel_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_PasswordLabel_Exists();
                        break;
                    }
                case "Assert_Scheduler_ResourcePicker_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_ResourcePicker_Exists();
                        break;
                    }
                case "Assert_Scheduler_RunTask_Checkbox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_RunTask_Checkbox_Exists();
                        break;
                    }
                case "Assert_Scheduler_Status_RadioButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_Status_RadioButton_Exists();
                        break;
                    }
                case "Assert_Scheduler_StatusLabe_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_StatusLabe_Exists();
                        break;
                    }
                case "Assert_Scheduler_TriggerLabel_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_TriggerLabel_Exists();
                        break;
                    }
                case "Assert_Scheduler_TriggerValue_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_TriggerValue_Exists();
                        break;
                    }
                case "Assert_Scheduler_UserAccountLabel_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_UserAccountLabel_Exists();
                        break;
                    }
                case "Assert_Scheduler_UsernameInput_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_UsernameInput_Exists();
                        break;
                    }
                case "Assert_Scheduler_Usernamelabel_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_Usernamelabel_Exists();
                        break;
                    }
                case "Assert_Scheduler_WorkflowInput_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_WorkflowInput_Exists();
                        break;
                    }
                case "Assert_Scheduler_WorkflowLabel_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Scheduler_WorkflowLabel_Exists();
                        break;
                    }
                case "Assert_Settings_ConnectButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Settings_ConnectButton_Exists();
                        break;
                    }
                case "Assert_Settings_ConnectControl_Edit_Button_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Settings_ConnectControl_Edit_Button_Exists();
                        break;
                    }
                case "Assert_Settings_ConnectControl_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Settings_ConnectControl_Exists();
                        break;
                    }
                case "Assert_Settings_LoggingTab_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Settings_LoggingTab_Exists();
                        break;
                    }
                case "Assert_Settings_ResourcePermissions_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Settings_ResourcePermissions_Exists();
                        break;
                    }
                case "Assert_Settings_SecurityTab_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Settings_SecurityTab_Exists();
                        break;
                    }
                case "Assert_Settings_ServerPermissions_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Settings_ServerPermissions_Exists();
                        break;
                    }
                case "Open_Context_Menu_OnDesignSurface":
                    {
                        Uimap.Open_Context_Menu_OnDesignSurface();
                        break;
                    }
                case "Tab_Context_Menu":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Tab_Context_Menu();
                        break;
                    }
                case "Click_SaveDialog_CancelButton":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_SaveDialog_CancelButton();
                        break;
                    }
                case "Assert_DebugInput_CancelButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_CancelButton_Exists();
                        break;
                    }

                case "Assert_DebugInput_DebugButton_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_DebugButton_Exists();
                        break;
                    }
                case "Assert_DebugInput_InputData_Field_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_InputData_Field_Exists();
                        break;
                    }
                case "Assert_DebugInput_InputData_Window_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_InputData_Window_Exists();
                        break;
                    }
                case "Assert_DebugInput_Json_Tab_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_Json_Tab_Exists();
                        break;
                    }
                case "Assert_DebugInput_Json_Window_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_Json_Window_Exists();
                        break;
                    }
                case "Assert_DebugInput_RememberCheckbox_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_RememberCheckbox_Exists();
                        break;
                    }
                case "Assert_DebugInput_ViewInBrowser_Button_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_ViewInBrowser_Button_Exists();
                        break;
                    }
                case "Assert_DebugInput_Window_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_Window_Exists();
                        break;
                    }
                case "Assert_DebugInput_Xml_Tab_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_Xml_Tab_Exists();
                        break;
                    }
                case "Assert_DebugInput_Xml_Window_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DebugInput_Xml_Window_Exists();
                        break;
                    }
                case "Click_Scheduler_Create_New_Task_Ribbon_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Scheduler_Create_New_Task_Ribbon_Button();
                        break;
                    }
                case "Click_Scheduler_Disable_Task_Radio_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Scheduler_Disable_Task_Radio_Button();
                        break;
                    }
                case "Click_Scheduler_EditTrigger_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Scheduler_EditTrigger_Button();
                        break;
                    }
                case "Click_Scheduler_Enable_Task_Radio_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Scheduler_Enable_Task_Radio_Button();
                        break;
                    }
                case "Click_Scheduler_ResourcePicker":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Scheduler_ResourcePicker();
                        break;
                    }
                case "Click_Scheduler_ResourcePicker_Button":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Scheduler_ResourcePicker_Button();
                        break;
                    }
                case "Click_Scheduler_Delete_Task":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Scheduler_Delete_Task();
                        break;
                    }
                case "Click_Scheduler_RunTask":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Scheduler_RunTask();
                        break;
                    }
                case "Click_Settings_Admin_ServerPermissions":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Settings_Admin_ServerPermissions();
                        break;
                    }
                case "Click_Settings_Contribute_ResourcePermissions":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Settings_Contribute_ResourcePermissions();
                        break;
                    }
                case "Click_Settings_Contribute_ServerPermissions":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Settings_Contribute_ServerPermissions();
                        break;
                    }
                case "Click_Settings_Execute_ResourcePermissions":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Settings_Execute_ResourcePermissions();
                        break;
                    }
                case "Click_Settings_ResourcePermissions_ResourcePicker":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Settings_ResourcePermissions_ResourcePicker();
                        break;
                    }
                case "Click_Settings_View_ResourcePermissions":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Settings_View_ResourcePermissions();
                        break;
                    }
                case "Click_Input_OnVariable_InVariableList":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Input_OnVariable_InVariableList();
                        break;
                    }
                case "Click_Input_OnRecordset_InVariableList":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Input_OnRecordset_InVariableList();
                        break;
                    }
                case "Click_Output_OnRecordset_InVariableList":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Output_OnRecordset_InVariableList();
                        break;
                    }
                case "Click_Output_OnVariable_InVariableList":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Output_OnVariable_InVariableList();
                        break;
                    }
                case "Click_ExpandAndStepIn_NestedWorkflow":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_ExpandAndStepIn_NestedWorkflow();
                        break;
                    }
                case "Click_Nested_Workflow_Name":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Nested_Workflow_Name();
                        break;
                    }
                case "Click_Cell_Highlights_Workflow_OnDesignSurface":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Click_Cell_Highlights_Workflow_OnDesignSurface();
                        break;
                    }

                case "Assert_Source_Server_Name_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Source_Server_Name_Exists();
                        break;
                    }
                case "Assert_Refresh_Button_Source_Server_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Refresh_Button_Source_Server_Exists();
                        break;
                    }
                case "Assert_Filter_Source_Server_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Filter_Source_Server_Exists();
                        break;
                    }
                case "Assert_Connect_Control_DestinationServer_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Connect_Control_DestinationServer_Exists();
                        break;
                    }
                case "Assert_Override_Count_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Override_Count_Exists();
                        break;
                    }
                case "Assert_NewResource_Count_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_NewResource_Count_Exists();
                        break;
                    }
                case "Assert_Source_Server_Edit_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Source_Server_Edit_Exists();
                        break;
                    }
                case "Assert_Connect_Button_Source_Server_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Connect_Button_Source_Server_Exists();
                        break;
                    }
                case "Assert_Edit_Button_Destination_Server_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Edit_Button_Destination_Server_Exists();
                        break;
                    }
                case "Assert_Connect_button_Destination_Server_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Connect_button_Destination_Server_Exists();
                        break;
                    }
                case "Assert_Connect_Control_SourceServer_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Connect_Control_SourceServer_Exists();
                        break;
                    }
                case "Assert_ShowDependencies_Button_DestinationServer_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_ShowDependencies_Button_DestinationServer_Exists();
                        break;
                    }
                case "Assert_ServiceLabel_DestinationServer_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_ServiceLabel_DestinationServer_Exists();
                        break;
                    }
                case "Assert_ServicesCount_Label_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_ServicesCount_Label_Exists();
                        break;
                    }
                case "Assert_SourceLabel_DestinationServer_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SourceLabel_DestinationServer_Exists();
                        break;
                    }
                case "Assert_SourceCount_DestinationServer_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SourceCount_DestinationServer_Exists();
                        break;
                    }
                case "Assert_NewResource_Label_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_NewResource_Label_Exists();
                        break;
                    }
                case "Assert_Override_Label_DestinationServer_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Override_Label_DestinationServer_Exists();
                        break;
                    }
                case "Assert_DeployButton_DestinationServer_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_DeployButton_DestinationServer_Exists();
                        break;
                    }
                case "Assert_SuccessMessage_Label_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_SuccessMessage_Label_Exists();
                        break;
                    }
                case "Assert_Explorer_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Explorer_Exists();
                        break;
                    }
                case "Assert_Explorer_ServerName_Exists":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Assert_Explorer_ServerName_Exists();
                        break;
                    }
                case "Right_Click_Context_Menu_InExplorer":
                    {
                        OutsideWorkflowDesignSurfaceUiMap.Right_Click_Context_Menu_InExplorer();
                        break;
                    }

                case "Assert_Mysql_Database_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_Mysql_Database_Large_View_Exists_OnDesignSurface();
                        break;
                    }
                case "Open_MySql_Database_Tool_Small_View":
                    {
                        Uimap.Open_MySql_Database_Tool_Small_View();
                        break;
                    }
                case "Open_Sql_Server_Tool_small_View":
                    {
                        Uimap.Open_Sql_Server_Tool_small_View();
                        break;
                    }
                case "Assert_SQL_Server_Database_Large_View_Exists_OnDesignSurface":
                    {
                        Uimap.Assert_SQL_Server_Database_Large_View_Exists_OnDesignSurface();
                        break;
                    }

                case "Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface();
                        break;
                    }
                case "Drag_Toolbox_MySql_Database_Onto_DesignSurface":
                    {
                        Uimap.Drag_Toolbox_MySql_Database_Onto_DesignSurface();
                        break;
                    }
                default:
                    {
                        Assert.Fail(p0 + " is not a recognized action recording.");
                        break;
                    }
            }
        }

        #region Properties and Fields

        UIMap Uimap
        {
            get
            {
                if ((_uiMap == null))
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        OutsideWorkflowDesignSurfaceUIMap OutsideWorkflowDesignSurfaceUiMap
        {
            get
            {
                if ((_outsideWorkflowDesignSurfaceUiMap == null))
                {
                    _outsideWorkflowDesignSurfaceUiMap = new OutsideWorkflowDesignSurfaceUIMap();
                }

                return _outsideWorkflowDesignSurfaceUiMap;
            }
        }

        private OutsideWorkflowDesignSurfaceUIMap _outsideWorkflowDesignSurfaceUiMap;

        static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.WorkingDirectory = Environment.CurrentDirectory;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }

        #endregion
    }
}
