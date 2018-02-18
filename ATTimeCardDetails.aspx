<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ATTimeCardDetails.aspx.vb" Inherits="Osi.AlChavo.WebUI.ATTimeCardDetails1"  MasterPageFile="~/MasterPages/AlChavo.Master" meta:resourcekey="PageResource1" uiculture="auto"  %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Osi.AlChavo.Common.Controls" Namespace="Osi.AlChavo.Common.Controls" TagPrefix="netsoft" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../../../App_Themes/ALCHAVO30/css/5.8_TM_custom_responsive.css" rel="stylesheet" />
    <style type="text/css">
        .timecard-profile {
            border-spacing: 10px;
            border-collapse: separate;
        }
        .profile-div {
            float: left;
        }
        .profile-name {
            color: blue;
            font-weight: bold;
            font-size:18px;
        }
        .a-bold {
            font-weight: bold;
        }
        .a-link {
            color: blue;
            text-decoration:underline;
            cursor:pointer;
        }
            .a-link:hover {
                color: red;
            }

        .localLiteral {
            font-family:Arial !important; 
            font-weight:bold;
        }

        .nameliteral {

            font-family:Arial !important; 
            font-weight:bold;
            font-size:18px;
            color: #045FB8;
        }
        a {
            cursor:pointer;
        }
        .recalculate {
           
            float:left;
        }
        #RdwPunchDetail {
            z-index:9999999;
        }
        .vent {
            z-index:9999999;
        }
        .warns {
            color: red;
        }
    </style>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    
    <telerik:RadScriptBlock ID="ScriptBlockGeneral" runat="server">

        <script type="text/javascript">
            function OnGridCreated(sender, eventArgs) {
                var HfFilter = document.getElementById('<%=HfFilter.ClientID %>');
                //var theGrid = $find('<%=NTSGeneralGridTC.ClientID%>');
                if (HfFilter.value == "true") {
                    //if(theGrid != null){
                        $find('<%=NTSGeneralGridTC.ClientID%>').get_masterTableView().showFilterItem();
                    //}
                } else {
                    //if (TheGrid != null) {
                        $find('<%=NTSGeneralGridTC.ClientID%>').get_masterTableView().hideFilterItem();
                    //}
                }
            }

            function showHideFilterItem() {
                var HfFilter = document.getElementById('<%=HfFilter.ClientID %>');
                if (HfFilter.value == "true") {
                    $find('<%=NTSGeneralGridTC.ClientID%>').get_masterTableView().hideFilterItem();
                     HfFilter.value = "false";
                 } else {
                     $find('<%=NTSGeneralGridTC.ClientID%>').get_masterTableView().showFilterItem();
                     HfFilter.value = "true";
                 }
                 return false;
            }

            function editPunch(PID, TID) {
                var wnd = $find('<%=RdwPunchDetail.ClientID()%>'); 
                wnd.setUrl('ATPunchDetailPopup.aspx?action=edit&ID=' + PID + '&TID=' + TID);
                wnd.show();
                return false;
            }
            
            function AddPunch() {
                var TID = $('#<%=ATC.ClientID%>').val();
                if ($("#<%=cbxApproved.ClientID%>").prop("checked") == true) {
                    return false;
                }
                editPunch(-1, TID);
            }
            function AddHours() {
                var TID = $('#<%=ATC.ClientID%>').val();
                if ($("#<%=cbxApproved.ClientID%>").prop("checked") == true) {
                    return false;
                }
                AddHoursPop(TID);
            }

            function AddHoursPop(TID) {
                var wnd = $find('<%=RdwAddHours.ClientID()%>');
                wnd.setUrl('ATAddHours.aspx?action=edit&TID=' + TID);
                wnd.show();
                return false;
            }
                
           
            </script>

        </telerik:RadScriptBlock> 
  
    <telerik:RadAjaxManager ID="AjaxManagerGeneral" runat="server" meta:resourcekey="AjaxManagerGeneralResource1">
        <ajaxsettings>
            <telerik:AjaxSetting AjaxControlID="PnlFilterContent">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="PnlFilterContent" />

                    <telerik:AjaxUpdatedControl ControlID="PnlGridContent" LoadingPanelID="LoadingPanelGeneral" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="PnlGridContent">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="PnlGridContent" LoadingPanelID="LoadingPanelGeneral" />
                 
                </UpdatedControls>
            </telerik:AjaxSetting>
        </ajaxsettings> 
   
    </telerik:RadAjaxManager> 

    <asp:Panel ID="PnlGridContent" runat="server" meta:resourcekey="PnlGridContentResource1">
     <div class="general-list" runat="server" id="PnlGeneralBtnTop" >
        <div class="profile-div">
        
         <table class="timecard-profile">
             <tr >
                 <td rowspan="3" style="vertical-align:top">
                       <telerik:RadBinaryImage runat="server" ID="ImgPhoto" ImageUrl="blank.png" ResizeMode="Fill"
                            Width="75px" Height="75px" AlternateText="No picture available" meta:resourcekey="ImgPhotoResource1"></telerik:RadBinaryImage>
                 </td>
                 <td>
                        <span class="nameliteral"><asp:Literal  ID="liName" runat="server" Text="Name "  meta:resourcekey="liName"  /></span> <asp:DropDownList  ID="ddlEmpList1" runat="server"  DataTextField="FullName" DataValueField="TimecardID" AutoPostBack="true" OnSelectedIndexChanged="ddlEmpList_SelectedIndexChanged" ></asp:DropDownList>
                       <asp:Label runat="server" Font-Size="Smaller" ID="laFilter" Text=""></asp:Label>
                 </td>
                 
             </tr>
             <tr>
                 <td>
                     <span class="localLiteral "><asp:Literal  ID="liFrom" runat="server" Text="FROM: " meta:resourcekey="liFrom"  /></span>
                     <span id="liFromDate" runat="server" ></span>
                     <asp:CheckBox ID="cbxOvernight" Checked="false" Visible="false" runat="server" Text="Overnight Shift" meta:resourcekey="cbxOvernight" />
                     <span class="localLiteral " style="text-align:left"><asp:Literal  ID="liTo" runat="server" Text="TO: " meta:resourcekey="liTo"  /></span><span id="liaToDate" runat="server" ></span>
                     <asp:Literal runat="server" Text="Approved" visible="false" meta:resourcekey="cbxApprovedRes3"></asp:Literal>
                 </td>
                 <td style="vertical-align:top">
                     
                     
                    <asp:CheckBox ID="cbxApproved" Checked="false" runat="server" OnCheckedChanged="cbxApproved_CheckedChanged" Text="Approved" meta:resourcekey="cbxApproved" AutoPostBack="true" />
                 </td>
                
             </tr>
             <tr>
                  <td class="recalculate">
                     <div class="general_btn">
                        <%--<asp:LinkButton  ID="lnkbtnRecalculate" runat="server" Text="Recalculate" meta:resourcekey="LnkBtnRecalculateres" ></asp:LinkButton>--%>
                         <asp:LinkButton  ID="lnkbtnAdHours" runat="server" Text="Add Hours" Visible="false" meta:resourcekey="LnkBtnAddHoursres" OnClientClick="AddHours();"></asp:LinkButton> &nbsp;
                         <asp:LinkButton  ID="lnkbtnAddHours" runat="server" Text="Request License" meta:resourcekey="lnkbtnAddHours" OnClick="lnkbtnAddHours_Click" ></asp:LinkButton>
                      </div>
                    
                 </td>
                 <td>
                     <div class="general_btn">
                    <asp:LinkButton ID="lnkbtnAddPunch" runat="server" Text="Add Punch" meta:resourcekey="lnkbtnAddPunch" OnClientClick="AddPunch();" ></asp:LinkButton>
                    </div>
                 </td>
                
             </tr>
         </table>
        </div>  
         <div class="general_btn">
            <asp:LinkButton ID="LnkBtnPrevious" runat="server" Text="Previous Period " meta:resourcekey="LnkBtnPrevious" ></asp:LinkButton>    
              <asp:LinkButton ID="LnkBtnNext" runat="server" Text="Next Period " meta:resourcekey="lnkbtn_NextPeriod" ></asp:LinkButton>    
         </div>
         <div class="clear"></div>
    </div>

     <span class="warns">   <asp:Literal ID="liMissPunch" runat="server" Text="Missing Punch" meta:resourcekey="liMissPunch" Visible="false"></asp:Literal> 
        <asp:Literal ID="liMismatchedPunchTypes" Visible="false"  runat="server" Text="Different Punch Types In Sequence" meta:resourcekey="liMismatchedPunchTypes"></asp:Literal>
      </span>
            <div class="dashboard-box" >

              <div class="tab-navbar" style="margin-bottom: 0; border-bottom: 0">
                <div class="view-list">


                </div>
            </div>

            <netsoft:NetsoftGrid ID="NTSGeneralGridTC" runat="server" EnableLinqExpressions="False"
                AllowSorting="False" AutoGenerateColumns="False" ContentBox="False"
                HeaderTitle="True" HeaderTitleText="Header Title de Grid"
                AllowPaging="True" AutomaticBuild="True" AllowFilteringByColumn="False"
                CellSpacing="0" GridLines="None" SelectColumnIndex="-1" >
                <clientsettings>
                   <ClientEvents OnGridCreated="OnGridCreated" /> 
                </clientsettings> 
                <groupingsettings casesensitive="false"></groupingsettings>
                <mastertableview >
                    <Columns>
                     
                       
                        <telerik:GridBoundColumn Datafield="2"  HeaderText="Monday" UniqueName="2" meta:resourcekey="Monday"   >
                           
                            <ItemStyle Width="14%" />
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn   Datafield="3" HeaderText="Tuesday" UniqueName="3" meta:resourcekey="Tuesday" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>
             

                        <telerik:GridBoundColumn HeaderText="Wednesday"  Datafield="4" UniqueName="4"  meta:resourcekey="Wednesday" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn HeaderText="Thursday"  Datafield="5" UniqueName="5" meta:resourcekey="Thursday" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn  HeaderText="Friday"  Datafield="6" UniqueName="6" meta:resourcekey="Friday"  >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn HeaderText="Saturday"  Datafield="7" UniqueName="7" meta:resourcekey="Saturday" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>
                       
                       <telerik:GridBoundColumn HeaderText="Sunday"  Datafield="1" UniqueName="1" meta:resourcekey="Sunday" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

 

                    </Columns>
                </mastertableview>
            </netsoft:NetsoftGrid>
            
                <%-- Hours Sum --%>
            <netsoft:NetsoftGrid ID="NTSGridHoursSum" runat="server" EnableLinqExpressions="False"
                AllowSorting="False" AutoGenerateColumns="False" ContentBox="False"
                HeaderTitle="True" HeaderTitleText="Header Title de Grid"
                AllowPaging="True" AutomaticBuild="True" AllowFilteringByColumn="False" Visible="false" 
                CellSpacing="0" GridLines="None" SelectColumnIndex="-1" >
                
                <groupingsettings casesensitive="false"></groupingsettings>
                <mastertableview >
                    <Columns>
                     
                       
                        <telerik:GridBoundColumn Datafield="2"  HeaderText="Monday" meta:resourcekey="lblMondayRes"   
                            UniqueName="Monday">
                           
                            <ItemStyle Width="14%" />
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn   Datafield="3" HeaderText="Tuesday"  meta:resourcekey="lblTuesdayRes" 
                            UniqueName="Tuesday">
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>
             

                        <telerik:GridBoundColumn HeaderText="Wednesday"  Datafield="4" UniqueName="Wednesday"  meta:resourcekey="lblWednesdayRes" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn HeaderText="Thursday"  Datafield="5" UniqueName="Thursday" meta:resourcekey="lblThursdayRes" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn  HeaderText="Friday"  Datafield="6" UniqueName="Friday" meta:resourcekey="lblFridayRes"  >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn HeaderText="Saturday"  Datafield="7" UniqueName="Saturday" meta:resourcekey="lblSaturdayRes" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>
                       
                       <telerik:GridBoundColumn HeaderText="Sunday"  Datafield="1" UniqueName="Sunday" meta:resourcekey="lblSundayRes" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

 

                    </Columns>
                </mastertableview>
            </netsoft:NetsoftGrid>

               

                <div class="content-form-title" id="ContentTitle" runat="server">
                <asp:Literal ID="LiHoursRegister" runat="server" Text="Hours Register" meta:resourcekey="LiHoursRegister" />
                <asp:Literal  ID="liApproved" runat="server" Visible="false" Text="Approved" meta:resourcekey="liApproved" />
            </div>

                <netsoft:NetsoftGrid ID="GeneralHoursGrid" runat="server" EnableLinqExpressions="False"
                AllowSorting="False" AutoGenerateColumns="False" ContentBox="False"
                HeaderTitle="True" HeaderTitleText="Header Title de Grid" 
                AllowPaging="True" AutomaticBuild="True" AllowFilteringByColumn="False"  
                CellSpacing="0" GridLines="None" SelectColumnIndex="-1" >
                
                <groupingsettings casesensitive="false"></groupingsettings>
                <mastertableview >
                    <Columns>
                        <telerik:GridBoundColumn Datafield="Department"  HeaderText="Department" meta:resourcekey="Department"   
                            UniqueName="Department">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn Datafield="SumOfHoursWorked"  HeaderText="Hours Worked" meta:resourcekey="SumOfHoursWorked"   
                          DataFormatString="{0:0.00}"    UniqueName="HoursWorked">
                        </telerik:GridBoundColumn>
                         <telerik:GridBoundColumn Datafield="SumOfRegularHours"  HeaderText="Regular"   
                          DataFormatString="{0:0.00}"    UniqueName="RegHours">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn Datafield="SumOfOverTimeExces8Hour"  HeaderText="Day OT" meta:resourcekey="SumOfOverTimeExces8Hour"   
                          DataFormatString="{0:0.00}"    UniqueName="OverEight">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn Datafield="SumOfOverTime40PlusHours"  HeaderText="Week OT" meta:resourcekey="SumOfOverTime40PlusHours"   
                           DataFormatString="{0:0.00}"   UniqueName="Overfort">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn Datafield="SumOfOverTime12HoursFlexiTime"  HeaderText="Rest"   
                          DataFormatString="{0:0.00}"    UniqueName="Overflex">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn Datafield="SumOfSevenConsecutiveDays"  HeaderText="Seventh" meta:resourcekey="SumOfSevenConsecutiveDays"   
                           DataFormatString="{0:0.00}"   UniqueName="OverSeventh">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn Datafield="SumOfOverTime24Hours"  HeaderText="24H" 
                          DataFormatString="{0:0.00}"    UniqueName="SumOfOverTime24Hours">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn Datafield="SumOfMealPenaltyHours"  HeaderText="Meal" meta:resourcekey="SumOfMealPenaltyHours"   
                           DataFormatString="{0:0.00}"   UniqueName="SumOfMealPenaltyHours">
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn Datafield="SumOfSun_Reg"  HeaderText="Sun Reg" meta:resourcekey="SumOfSun_Reg"   
                           DataFormatString="{0:0.00}"   UniqueName="SumOfSun_Reg">
                        </telerik:GridBoundColumn>
                       
                        
                    </Columns>
                </mastertableview>
                </netsoft:NetsoftGrid>
                 <div class="content-form-title" id="Div1" runat="server">
                <asp:Literal ID="liAdditionalHOurs" runat="server" Text="Additional Hours" meta:resourcekey="liAdditionalHOurs" />
               
            </div>

                 <netsoft:NetsoftGrid ID="AdditionalGrid" runat="server" EnableLinqExpressions="False"
                AllowSorting="False" AutoGenerateColumns="False" ContentBox="False"
                HeaderTitle="True" HeaderTitleText="Header Title de Grid" 
                AllowPaging="True" AutomaticBuild="True" AllowFilteringByColumn="False"  
                CellSpacing="0" GridLines="None" SelectColumnIndex="-1" >
                
                <groupingsettings casesensitive="false"></groupingsettings>
                <mastertableview DataKeyNames="ID">
                    <Columns>
                        <telerik:GridBoundColumn Datafield="ID" visible="false" HeaderText="ID" meta:resourcekey="lblidRes"   
                            UniqueName="ID">
                        </telerik:GridBoundColumn> 

                         <telerik:GridButtonColumn meta:resourcekey="Delete" CommandName="Delete" Text="Delete" UniqueName="DeleteColumn" />
                           
                          
                        
                        <telerik:GridBoundColumn Datafield="EarningID"  HeaderText="Earning" meta:resourcekey="EarningID"   
                            UniqueName="Earning">
                           
                           
                        </telerik:GridBoundColumn>

                         <telerik:GridBoundColumn Datafield="HoursAmount"  HeaderText="Hours Amount" meta:resourcekey="HoursAmount"   
                          DataFormatString="{0:0.00}"    UniqueName="Hours">
                           
                           
                        </telerik:GridBoundColumn>

                         <telerik:GridBoundColumn Datafield="AppliedToDate" DataFormatString="{0:MM/dd/yyyy}"  HeaderText="On" meta:resourcekey="AppliedToDate"   
                            UniqueName="AppliedToDate">
                           
                            
                        </telerik:GridBoundColumn>
                        </Columns>
                    </mastertableview>
               </netsoft:NetsoftGrid> 
            
             <div  >
               <%--<div class="quest-form" style="border: 0;">
                <div class="quest-column">
                    <div class="quest-column-left-view">
                        
                            <asp:Literal runat="server" Text=""  ></asp:Literal>
                    </div>
                    <div class="quest-column-right-view">
                        <ul>
                            
                            <li class="liDynamic">

                                <asp:Literal  runat="server" Text="" ></asp:Literal>

                            </li>
                        </ul>
                    </div>
                </div>
                <div class="quest-column" style="border: 0;">
                    <div class="quest-column-left-view">
                        
                            <asp:Literal ID="Literal1" runat="server" Text=""  meta:resourcekey="LiSomeBlankRes"></asp:Literal>

                    </div>
                    <div class="quest-column-right-view" >
                        <ul>
                            <li class="liDynamic">

                                <asp:Literal ID="Literal2" runat="server" Text="" ></asp:Literal>

                            </li>
                        </ul>
                    </div>
                </div>

                 <netsoft:NetsoftGrid ID="NTSGridSchedule" runat="server" EnableLinqExpressions="False"
                AllowSorting="False" AutoGenerateColumns="False" ContentBox="False"
                HeaderTitle="True" HeaderTitleText="Header Title de Grid"
                AllowPaging="True" AutomaticBuild="True" AllowFilteringByColumn="False"
                CellSpacing="0" GridLines="None" SelectColumnIndex="-1" >
                <clientsettings>
                   <ClientEvents OnGridCreated="OnGridCreated" /> 
                </clientsettings> 
                <groupingsettings casesensitive="false"></groupingsettings>
                <mastertableview >
                    <Columns>
                     
                       
                        <telerik:GridBoundColumn DataField="2"   HeaderText="Monday" meta:resourcekey="lblMondayRes"   
                            UniqueName="2">
                            
                            <ItemStyle Width="14%" />
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn  DataField="3"  HeaderText="Tuesday"  meta:resourcekey="lblTuesdayRes" 
                            UniqueName="3">
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>
             

                        <telerik:GridBoundColumn HeaderText="Wednesday" DataField="4"  UniqueName="4"  meta:resourcekey="lblWednesdayRes" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn HeaderText="Thursday" DataField="5"  UniqueName="5" meta:resourcekey="lblThursdayRes" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn  HeaderText="Friday" DataField="6"  UniqueName="6" meta:resourcekey="lblFridayRes"  >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn HeaderText="Saturday" DataField="7"  UniqueName="7" meta:resourcekey="lblSaturdayRes" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>
                       
                       <telerik:GridBoundColumn HeaderText="Sunday" DataField="1"  UniqueName="1" meta:resourcekey="lblSundayRes" ColumnGroupName="1" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

 

                    </Columns>
                </mastertableview>
            </netsoft:NetsoftGrid>
                <div class="clear"></div>
              </div>  --%>  
                 <netsoft:NetsoftGrid ID="NTSGridSchedule" runat="server" EnableLinqExpressions="False"
                AllowSorting="False" AutoGenerateColumns="False" ContentBox="False"
                HeaderTitle="True" HeaderTitleText="Header Title de Grid"
                AllowPaging="True" AutomaticBuild="True" AllowFilteringByColumn="False"
                CellSpacing="0" GridLines="None" SelectColumnIndex="-1" >
                <clientsettings>
                   <ClientEvents OnGridCreated="OnGridCreated" /> 
                </clientsettings> 
                <groupingsettings casesensitive="false"></groupingsettings>
                <mastertableview >
                    <Columns>
                     
                       
                        <telerik:GridBoundColumn DataField="2"   HeaderText="Monday" meta:resourcekey="Monday"   
                            UniqueName="2">
                            
                            <ItemStyle Width="14%" />
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn  DataField="3"  HeaderText="Tuesday"  meta:resourcekey="Tuesday" 
                            UniqueName="3">
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>
             

                        <telerik:GridBoundColumn HeaderText="Wednesday" DataField="4"  UniqueName="4"  meta:resourcekey="Wednesday" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn HeaderText="Thursday" DataField="5"  UniqueName="5" meta:resourcekey="Thursday" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn  HeaderText="Friday" DataField="6"  UniqueName="6" meta:resourcekey="Friday"  >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

                        <telerik:GridBoundColumn HeaderText="Saturday" DataField="7"  UniqueName="7" meta:resourcekey="Saturday" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>
                       
                       <telerik:GridBoundColumn HeaderText="Sunday" DataField="1"  UniqueName="1" meta:resourcekey="Sunday" ColumnGroupName="1" >
                            <ColumnValidationSettings>
                                <ModelErrorMessage Text="" />
                            </ColumnValidationSettings>

                            <ItemStyle CssClass="NetsoftGridAdditionalColumn" Width="14%" />
                            <FooterStyle CssClass="NetsoftGridAdditionalColumn" />
                            <HeaderStyle CssClass="NetsoftGridHeaderResponsive" />
                        </telerik:GridBoundColumn>

 

                    </Columns>
                </mastertableview>
            </netsoft:NetsoftGrid>  
           </div>

             

                <br />
                 <div style="float:right">
                        <asp:LinkButton CssClass="btn" ID="lnkBtnEdit" runat="server" Text="Edit Schedule" meta:resourcekey="lnkBtnEdit"  ></asp:LinkButton>
                     </div>   
                
                <div class="clear"></div>
        </div>
        <footer>
            <div class="general_btn">
                <asp:LinkButton ID="lnkBtn_Previous" runat="server" Text="Previous Period" meta:resourcekey="lnkBtn_Previous" ></asp:LinkButton>
                <asp:LinkButton ID="lnkbtn_NextPeriod" runat="server" Text="Next Period" meta:resourcekey="lnkbtn_NextPeriod" ></asp:LinkButton>
            </div>
            <div class="clear"></div>
        </footer>
         <asp:HiddenField ID="HfFilter" runat="server" />
    <asp:HiddenField ID="HdfclosePopUp" runat="server" />
    <asp:HiddenField ID="ATC" runat="server" />
        <asp:HiddenField ID="Warn" runat="server" ClientIDMode="Static" />
    </asp:Panel> 
   
    <telerik:RadAjaxLoadingPanel ID="LoadingPanelGeneral" runat="server" Skin="Default" Transparency="30" ZIndex="99999" meta:resourcekey="LoadingPanelGeneralResource1"></telerik:RadAjaxLoadingPanel>

     <telerik:RadWindow ID="RdwPunchDetail" Modal="True" Title="Punch Detail" runat="server"  EnableShadow="True"  Width="1300px" Height="600px" Behaviors="Close,Reload" VisibleStatusbar="false" VisibleTitlebar="true" meta:resourcekey="RdwPunchDetail" style="z-index:9999999999" >           
     
     </telerik:RadWindow>

    <telerik:RadWindow ID="RdwAddHours" Modal="True" Title="Add Hours" runat="server"  EnableShadow="True"  Width="1000px" Height="400px" Behaviors="Close,Reload" VisibleStatusbar="false" VisibleTitlebar="true" meta:resourcekey="RdwAddHours" style="z-index:9999999999" >           
     
     </telerik:RadWindow>

    <telerik:RadWindow ID="RdwWrn" Modal="True" ClientIDMode="Static" Title="Missing Config" runat="server"  EnableShadow="True"  Width="800px" Height="275px" Behaviors="Close" VisibleStatusbar="false" VisibleTitlebar="true" meta:resourcekey="RdwWrn" style="z-index:9999999999" >           
     
     </telerik:RadWindow>

    <script type="text/javascript" >
     $(document).ready(function () {
            
         setTimeout(function () {
          
                if ($('#Warn').val() == "1") {

                    var wnd = $find('RdwWrn');
                    wnd.setUrl('ATTCWarning.aspx');
                    $('#Warn').val('0');
                    wnd.show();
                    return false;               
                }
         }, 3000);
     });
    </script>
</asp:Content>
