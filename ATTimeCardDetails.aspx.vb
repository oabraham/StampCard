Imports Osi.AlChavo.AT.DataProviders
'Imports Telerik.Web.UI
'Imports Osi.AlChavo.DataProviders
Imports System.IO
Imports System.Globalization
Imports System.Threading

Public Class ATTimeCardDetails1
    Inherits System.Web.UI.Page

    Dim CompPenalties As Dictionary(Of String, Double)
    Dim Punches As Dictionary(Of TimeSpan, Boolean)
    Dim StartDate As DateTime = DateTime.Now
    Dim EndDate As DateTime = DateTime.Now
    Dim TimeCardNextID As Integer? = 0
    Dim TimeCardPreviousID As Integer? = 0
    Dim TimeCardID As Integer? = 0
    Dim EmployeeScheduleList As List(Of ATGetEmployeeScheduleByPeriod_Result) = New List(Of ATGetEmployeeScheduleByPeriod_Result)

    ''' <summary >
    ''' This method validates if the logged user is either an admin, supervisor or owner of the timecard.
    ''' Stored Procedure: ATLookAdmin, ATAttendanceSupervisorRole, ATGetESSInfo, ATGetEmployeeInfoByUserSetup
    ''' </summary>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            CheckTimecardVersion()
            SetSecurity()
            If Session("TimeCardSummaryMode") IsNot Nothing Then
                Session.Remove("TimeCardSummaryMode")
            End If
            If Session("IsATSuperVisor") IsNot Nothing Then
                Session.Remove("IsATSuperVisor")
            End If
            If Session("IsATAdmin") IsNot Nothing Then
                Session.Remove("IsATAdmin")
            End If
            Session.Add("TimeCardSummaryMode", 1) 'TEMP


            Dim dp As New AttendanceDataProvider

            Dim IsSuperRole As Boolean = dp.ATIsSuperRole(Session("CompanyID"), New Guid(Session("UserGuidID").ToString))
            Dim IsSuperVisor As Boolean = dp.ATIsAttendanceSupervisor(Session("CompanyID"), New Guid(Session("UserGuidID").ToString))
            Session.Add("IsATSuperVisor", IsSuperVisor)
            Session.Add("IsATAdmin", IsSuperRole)
            If Not (IsSuperRole Or IsSuperVisor) Then
                cbxApproved.Visible = False
                liApproved.Visible = False
                ddlEmpList1.Visible = False
                If Not dp.ATIDMatch(Session("CompanyID"), New Guid(Session("UserGuidID").ToString), TimeCardID) Then
                    'check self service
                    Dim ESSList As List(Of ATGetESSInfo_Result) = dp.ATGetEmpSSPInfo(Session("CompanyID"), New Guid(Session("UserGuidID").ToString))
                    If ESSList.Count = 0 Then

                        Dim UserSetupInfo As List(Of ATGetEmployeeInfoByUserSetup_Result) = dp.ATGetEmployeeInfoByUserSetup(Session("CompanyID"), New Guid(Session("UserGuidID").ToString))
                        If UserSetupInfo.Count > 0 Then
                            'check id match
                            If Not dp.ATIDMatchByEntryNum(Session("CompanyID"), UserSetupInfo(0).EntryNum, TimeCardID) Then
                                Response.Redirect("~/WebPages/General/Attendance/ATTimeCardsGeneral.aspx")
                            End If
                        Else
                            dp = Nothing
                            Response.Redirect("~/WebPages/General/Attendance/ATTimeCardsGeneral.aspx")
                        End If

                    Else
                        'check id match
                        If Not dp.ATIDMatchByEntryNum(Session("CompanyID"), ESSList(0).EntryNum, TimeCardID) Then
                            Response.Redirect("~/WebPages/General/Attendance/ATTimeCardsGeneral.aspx")
                        End If
                    End If

                End If
            Else

                FillEmployeeDropDown(TimeCardID, IsSuperRole, IsSuperVisor, dp)
                If IsSuperRole Then
                    'lnkbtnAdHours.Visible = True
                End If
            End If

            dp = Nothing

            ATC.Value = TimeCardID
            DoCalcs(TimeCardID)


        End If
    End Sub

    ''' <summary >
    ''' Loads the employee name dropdown.
    ''' Stored Procedure: ATGetEmployeeTCListByDpt, ATGetEmployeeTCSuperDpt, ATGetEmployeeTCListAll, ATGetEmployeeTCSuper
    ''' </summary>
    ''' <remarks>The employee list will vary depending of different factors:
    ''' If an admin is browsing, it will display all the employees that have a timecard for that period; this list comes filtered from the general page if it was filtered by department.
    ''' If it is a supervisor, it will display the employees from his assigned department, also can be filtered by department from the timecards general page.
    ''' A regular user will only see his name on the dropdown.</remarks>
    Public Sub FillEmployeeDropDown(ByVal TimecardID As Integer, ByVal IsSuperRole As Boolean, ByVal IsSuperVisor As Boolean, ByVal dp As AttendanceDataProvider)
        'Fill Employee Dropdown
        liName.Visible = False
        laFilter.Text = ""
        If Session("DepartmentIDFilter") IsNot Nothing Then
            'Get emp filtered
            laFilter.Text = "Dpt " & Session("DepartmentIDFilter")
            'liName.Text = "Dpt " & Session("DepartmentIDFilter")
            If IsSuperRole Then
                ddlEmpList1.DataSource = dp.ATGetEmpTCListAllDpt(Session("CompanyID"), TimecardID, Session("DepartmentIDFilter"))
            Else
                ddlEmpList1.DataSource = dp.ATGetEmpTCSuperDpt(Session("CompanyID"), TimecardID, Session("DepartmentIDFilter"), New Guid(Session("UserGuidID").ToString))
            End If
        ElseIf IsSuperRole Then
            'Get all Emps
            ddlEmpList1.DataSource = dp.ATGetEmpTCListAll(Session("CompanyID"), TimecardID)
        ElseIf IsSuperVisor Then
            'Get Super Emps
            ddlEmpList1.DataSource = dp.ATGetEmpTCSuper(Session("CompanyID"), TimecardID, New Guid(Session("UserGuidID").ToString))
        End If

        ddlEmpList1.DataBind()
        ddlEmpList1.SelectedValue = TimecardID

    End Sub

    ''' <summary >
    ''' Setups the page security.
    ''' </summary>
    Private Sub SetSecurity()
        Dim AddState As Boolean = Osi.AlChavo.WebUI.ApplicationMethods.SECValidatePermission(SecurityManagement.DataProviders.Security.Actions.Add)
        'lnkbtnAddHours.Visible = AddState
        'lnkbtnAddPunch.Visible = AddState
    End Sub

    ''' <summary >
    ''' This function does the time register calculations and loads the grids.
    ''' Stored Procedure: ATGetCompanyPenalties, ATPopulateTimeRegisterDisplay, ATGetCompanyMealPenaltyHours, ATCheckPayrollStart,
    ''' ATGetEmployeeEntryNumByTimeCard, ATGetEmployeeName, ATGetWeekPayrollPeriodByTimeCardID, ATTimeCardPunchesByTCID, ATMisMatchedPunchTypes
    ''' , ATPopulateTimeRegisterDisplay, ATGetEmployeeSchedule, ATGetManualHours, ATMissingPunch
    ''' </summary>
    Public Function DoCalcs(ByVal TimeCardID As Integer?) As Boolean
        Dim dp As New Osi.AlChavo.AT.DataProviders.AttendanceDataProvider
        Dim TimeCardSummaryClass As New ATTimeCardSummary
        Dim TimeRegisterList As List(Of ATPopulateTimeRegisterDisplay_Result)

        NTSGeneralGridTC.DataSource = New DataTable
        NTSGeneralGridTC.DataBind()

        Dim UserMode As String = Session("TimeCardSummaryMode") ' User Mode 1 Para single user Mode 111 para admin/get all empleados de la compania 

        If UserMode Is Nothing Then
            Return False
        End If

        Dim OnlyEmp As Boolean = True
        Dim EmployeeEntryNum As Integer? = 0
        Dim AllEmployees As Boolean = False

        'Penalty Retrieve
        Dim CompanyPenaltyList As List(Of ATGetCompanyPenalties_Result) = dp.GetCompanyPenalties(Session("CompanyID"))

        CompPenalties = TimeCardSummaryClass.ConvertToDictionary(CompanyPenaltyList)

        'Amount of Meal Penalty Hours
        Dim CompMealPenaltyHoursList As List(Of Double?) = dp.ATGetMealPenaltyHours(Session("CompanyID")), CompMealPenaltyHours As Double? = 0
        If CompMealPenaltyHoursList.Count > 0 Then
            CompMealPenaltyHours = CompMealPenaltyHoursList(0)
            If CompMealPenaltyHours Is Nothing Then
                CompMealPenaltyHours = 0
            End If
        End If

        cbxApproved.Enabled = True

        If IsApproved() Then
            cbxApproved.Checked = True
            lnkbtnAddPunch.Enabled = False
        Else
            cbxApproved.Checked = False
            lnkbtnAddPunch.Enabled = True
        End If

        'Company Payroll Start Configured
        Dim CompanyPayrollStart As List(Of Integer?) = dp.CheckPayrollStart(Session("CompanyID"))
        If CompanyPayrollStart.Count = 0 Then
            Return False
        End If

        'Get Employee Entry Num
        Dim EmployeeEntryNumList As List(Of Integer?) = dp.ATGetEmployeeEntryNumByTimeCardId(Session("CompanyID"), TimeCardID)

        If EmployeeEntryNumList.Count = 0 Then
            'No entrynum with this card
            Return False
        End If

        EmployeeEntryNum = EmployeeEntryNumList(0).Value

        'Employee Name
        Dim EmpNameList As List(Of String) = dp.ATGetFullEmployeeName(Session("CompanyID"), EmployeeEntryNum)
        If EmpNameList.Count > 0 Then
            liName.Text = EmpNameList(0)
        End If

        'Get Start and End Date Selected Period
        Dim PayrollPeriod As Integer? = 0
        Dim PayrollPeriodList As List(Of ATGetWeekPayrollPeriodByTimeCardID_Result) = dp.ATGetTimeCardWeek(Session("CompanyID"), TimeCardID)

        StartDate = PayrollPeriodList(0).WeekStart
        EndDate = PayrollPeriodList(0).WeekEnd
        liFromDate.InnerText = StartDate.ToString.Split(" ")(0)
        liaToDate.InnerText = EndDate.ToString.Split(" ")(0)

        'Get Timecard Punches
        Dim ScheduleList As List(Of ATTimeCardPunchesByTCID_Result) = dp.ATGetTimeCardPunchesByTimeCardID(Session("CompanyID"), TimeCardID)
        Dim ScheduleListTemp As New List(Of ATTimeCardPunchesByTCID_Result)
        Copy_Obj(ScheduleList, ScheduleListTemp)

        PopulateActualAssistanceGrid(TimeCardID, ScheduleListTemp)

        'Missing Punches match punch types 
        Dim MissPunchList As List(Of ATMissingPunch_Result) = dp.ATGetMissingPunchesSingle(Session("CompanyID"), StartDate, EndDate, EmployeeEntryNum)
        'Dim PunchTypeMatchList As List(Of ATMisMatchedPunchTypes) = dp.ATGetMismatchPunchTypesSingle(Session("CompanyID"), StartDate, EndDate, EmployeeEntryNum)

        If MissPunchList.Count > 0 Then
            'Missing punches warn
            liMissPunch.Visible = True
        Else
            liMissPunch.Visible = False
        End If

        'If PunchTypeMatchList.Count > 0 Then
        '    'Punch types mismatch
        '    liMismatchedPunchTypes.Visible = True
        'Else
        '    liMismatchedPunchTypes.Visible = False
        'End If

        'Get Timecard Summary

        TimeRegisterList = dp.ATPopulateTimeRegisterAlternate(Session("CompanyID"), OnlyEmp, EmployeeEntryNum, CompPenalties.ContainsKey("Meal"), _
                                       CompMealPenaltyHours, CompPenalties.ContainsKey("24H"), CompPenalties.ContainsKey("Flex"), CompPenalties.ContainsKey("Over8"), CompPenalties.ContainsKey("Over40"), _
                                      CompPenalties.ContainsKey("Seven"), CompPenalties.ContainsKey("SevenClose"), StartDate, EndDate)

        If TimeRegisterList.Count > 0 Then
            FillHourLabels(TimeRegisterList)
        Else
            'No register with this period
            GeneralHoursGrid.DataSource = New DataTable()
            GeneralHoursGrid.DataBind()

            If Not Session("GeneralHours") Is Nothing Then
                Session.Remove("GeneralHours")
            End If

            Session.Add("GeneralHours", GeneralHoursGrid.DataSource)
            ClearHourLabels()
        End If

        'Holiday Hours
        'liHolidayHoursRegistered.Text = SumHolidayHours(ScheduleList)
        'Populate sum of hours grid
        PopulateHoursSumGrid(ScheduleList)

        If ScheduleList.Count > 0 Then
            liApproved.Visible = ScheduleList(0).Approved
        End If

        'Get Employee Schedule
        Dim TotalWeeklyHours As Double = 0
        EmployeeScheduleList = dp.ATGetEmployeeSchedule(Session("CompanyID"), EmployeeEntryNum, StartDate)
        If EmployeeScheduleList.Count = 0 Then
            'No schedule
            NTSGridSchedule.DataSource = New DataTable
            NTSGridSchedule.DataBind()
        Else

            TotalWeeklyHours = Math.Round(SumWeekHours(EmployeeScheduleList), 2, MidpointRounding.AwayFromZero)
            PopulateScheduleGrid(EmployeeScheduleList)

        End If

        'Get Additional Hours
        Dim AdditionalHoursList As List(Of ATGetManualHours_Result) = dp.ATGetManualHours(Session("CompanyID"), StartDate, EndDate, TimeCardID)
        AdditionalGrid.DataSource = AdditionalHoursList
        AdditionalGrid.DataBind()

        If Not Session("AdditionalHours") Is Nothing Then
            Session.Remove("AdditionalHours")
        End If

        Session.Add("AdditionalHours", AdditionalGrid.DataSource)

        FillEmployeeDropDown(TimeCardID, Session("IsATAdmin"), Session("IsATSuperVisor"), dp)

        Try

            Dim FormatsArray() As String = {".jpg", ".jpeg", ".png"} 'Supported extensions
            ImgPhoto.ImageUrl = ""
            For Each FormatImage In FormatsArray
                Dim ImagePath As String = dp.GetImagesFileVirtualPath(Session("CompanyID")) & "Images/" &
                                "PA_" & Session("CompanyID").ToString & "_Employee_" & dp.ATGetEmployeeIDFromEntryNum(EmployeeEntryNum) & FormatImage
                If File.Exists(Server.MapPath(ImagePath)) Then
                    ImgPhoto.ImageUrl = ImagePath
                    'DirectCast(FrmDetail.FindControl("imgBttnDelete"), ImageButton).Visible = True
                    Exit For
                End If
            Next

            If ImgPhoto.ImageUrl = String.Empty Or ImgPhoto.ImageUrl = "blank.png" Then
                ImgPhoto.ImageUrl = "~/App_Themes/ALCHAVO30/img/avatar.png"
                'DirectCast(FrmDetail.FindControl("imgBttnDelete"), ImageButton).Visible = False
            End If

        Catch
            ImgPhoto.ImageUrl = "~/App_Themes/ALCHAVO30/img/avatar.png"
        End Try

        dp = Nothing

        Return True

    End Function

    ''' <summary >
    ''' Loads the daily hour grid for each day.
    ''' </summary>
    Public Sub FillHourLabels(ByRef TimeRegisterList As List(Of ATPopulateTimeRegisterDisplay_Result))

        'liSumOfHoursWorked.Text = TimeRegisterList(0).SumOfHoursWorked.ToString
        'liRegularHoursWorked.Text = TimeRegisterList(0).SumOfRegularHours.ToString
        'liOvertimeHoursWorked.Text = (TimeRegisterList(0).SumOfOverTimeExces8Hour +
        '                              TimeRegisterList(0).SumOfOverTime12HoursFlexiTime +
        '                              TimeRegisterList(0).SumOfOverTime24Hours +
        '                              TimeRegisterList(0).SumOfOverTime40PlusHours +
        '                              TimeRegisterList(0).SumOfSevenConsecutiveDays +
        '                              TimeRegisterList(0).SumOfSixConsecutiveDays +
        '                              TimeRegisterList(0).SumOfSundayOT).ToString & " DPT: " & TimeRegisterList(0).Department
        'liSickHours.Text = TimeRegisterList(0).SumOfSickHours.ToString
        'liVacationHoursRegistered.Text = TimeRegisterList(0).SumOfVacHours.ToString

        GeneralHoursGrid.DataSource = TimeRegisterList
        GeneralHoursGrid.DataBind()

        If Not Session("GeneralHours") Is Nothing Then
            Session.Remove("GeneralHours")
        End If

        Session.Add("GeneralHours", GeneralHoursGrid.DataSource)

    End Sub

    Public Sub ClearHourLabels()
        'liSumOfHoursWorked.Text = "0"
        'liRegularHoursWorked.Text = "0"
        'liOvertimeHoursWorked.Text = "0"
        'liSickHours.Text = "0"
        'liVacationHoursRegistered.Text = "0"
    End Sub

    ''' <summary >
    ''' Sums the amount of daily hours per day.
    ''' </summary>
    ''' <param name="ScheduleList" >The list of week punchs.</param>
    ''' <returns> The amount of hours scheduled for the week.</returns>
    Public Function SumWeekHours(ByRef ScheduleList As List(Of ATGetEmployeeScheduleByPeriod_Result)) As Double

        Dim WeekHours As Double = 0
        Dim PunchInHour As TimeSpan

        For Each element As ATGetEmployeeScheduleByPeriod_Result In ScheduleList

            If element.IsPunchIn Then
                PunchInHour = element.Hoursort
            Else

                If PunchInHour <> Nothing Then

                    If PunchInHour > element.Hoursort Then
                        WeekHours = WeekHours + ((New TimeSpan(23, 59, 59)).Subtract(PunchInHour).TotalHours + (New TimeSpan(0, 0, 0)).Add(element.Hoursort).TotalHours)
                    Else
                        WeekHours = WeekHours + CType(element.Hoursort, TimeSpan).Subtract(PunchInHour).TotalHours
                    End If

                    PunchInHour = Nothing
                Else

                    'Missing punch in
                End If

            End If

        Next

        If ScheduleList.Count > 0 Then
            If Not ScheduleList(0).IsPunchIn Then
                WeekHours = WeekHours + ((New TimeSpan(23, 59, 59)).Subtract(ScheduleList(ScheduleList.Count - 1).Hoursort).TotalHours +
                                         (New TimeSpan(0, 0, 0)).Add(ScheduleList(0).Hoursort).TotalHours)
            End If
        End If

        Return WeekHours

    End Function

    ''' <summary >
    ''' Fills the top grid with the punch hours and departments.
    ''' </summary>
    ''' <param name="ScheduleList" >The list of punchs for the week.</param>
    ''' <param name="TimeCardID" >The timecard ID from tblATTimecards.ID </param>
    Public Sub PopulateActualAssistanceGrid(ByVal TimeCardID As Integer?, ByVal ScheduleList As List(Of ATTimeCardPunchesByTCID_Result))

        Dim dp As New AttendanceDataProvider

        'Get Timecard Punches

        Dim dt As New DataTable, weekday As Integer, prevweekday As Integer = 0, punchmismatchbuffer = 0
        Dim ScheduleListMaxIndex = ScheduleList.Count - 1, PunchDict As New Dictionary(Of Boolean, String)
        Dim StyleOptions As New Dictionary(Of Boolean, String), dateform As DateTime
        PunchDict.Add(True, "IN")
        PunchDict.Add(False, "OUT")
        StyleOptions.Add(True, " style='color:red;font-weight:bold' ")
        StyleOptions.Add(False, " style='color:green' ")
        Dim maxmismatch As Integer = 35

        For Int As Integer = 1 To 7
            dt.Columns.Add(Int.ToString, GetType(String))
        Next

        'Autopunch Commented No go
        ''Sum From 12Am if first punch is punch out
        If ScheduleList.Count > 0 And cbxOvernight.Checked Then
            If Not ScheduleList(0).IsPunchIn Then

                Dim autopuncham As New ATTimeCardPunchesByTCID_Result()
                autopuncham.Hour = New TimeSpan(0, 0, 0)
                autopuncham.ID = -1
                autopuncham.IsPunchIn = True
                autopuncham.PType = "Auto"
                ScheduleList.Insert(0, autopuncham)

            End If

            'Sum to 11:59pm if last punch is punch  in
            If ScheduleList(ScheduleList.Count - 1).IsPunchIn Then

                Dim autopunchpm As New ATTimeCardPunchesByTCID_Result()
                autopunchpm.Hour = New TimeSpan(11, 59, 59)
                autopunchpm.ID = -1
                autopunchpm.IsPunchIn = False
                autopunchpm.PType = "Auto"
                ScheduleList.Add(autopunchpm)

            End If

        End If

        Dim arow As DataRow = dt.NewRow

        While ScheduleList.Count <> 0

            For Int As Integer = 0 To ScheduleListMaxIndex Step 1
                weekday = ScheduleList(Int).DayOfWeek

                If weekday <> prevweekday Then

                    If prevweekday > ScheduleList(Int).DayOfWeek Then
                        dt.Rows.Add(arow)
                        arow = dt.NewRow()
                    End If
                    dateform = New DateTime(2012, 1, 1) + ScheduleList(Int).Hour
                    arow(ScheduleList(Int).DayOfWeek.ToString) = String.Format("<a onclick='editPunch(" + ScheduleList(Int).ID.ToString + ", " + TimeCardID.ToString + ") ' " + StyleOptions(ScheduleList(Int).Edited) + " >" + dateform.ToString("h:mm tt") + _
                                                                               " - " + (PunchDict(ScheduleList(Int).IsPunchIn) + " ").Substring(0, 3) + "&nbsp;" + ScheduleList(Int).Dpt + "</a>").ToString
                    prevweekday = ScheduleList(Int).DayOfWeek
                    ScheduleList.Remove(ScheduleList(Int))
                    ScheduleListMaxIndex = ScheduleListMaxIndex - 1
                    Int = Int - 1
                    punchmismatchbuffer = 0

                Else
                    punchmismatchbuffer = punchmismatchbuffer + 1
                    If punchmismatchbuffer = maxmismatch Then
                        Exit While
                    End If

                End If

                If ScheduleListMaxIndex = Int Then
                    Exit For
                End If

            Next

        End While

        dt.Rows.Add(arow)

        If punchmismatchbuffer = maxmismatch Then
            For Each elemento As ATTimeCardPunchesByTCID_Result In ScheduleList
                Dim somerow As DataRow = dt.NewRow
                dateform = New DateTime(2012, 1, 1) + elemento.Hour
                somerow(elemento.DayOfWeek.ToString) = String.Format("<a onclick='editPunch(" + elemento.ID.ToString + ", " + TimeCardID.ToString + ") ' " + StyleOptions(elemento.Edited) + " >" + dateform.ToString("h:mm tt") + _
                                                                               " - " + (PunchDict(elemento.IsPunchIn) + " ").Substring(0, 3) + "&nbsp;" + elemento.Dpt + "</a>").ToString
                dt.Rows.Add(somerow)
            Next
        End If

        NTSGeneralGridTC.DataSource = dt
        NTSGeneralGridTC.DataBind()

        If Not Session("TimecardAssistanceGrid") Is Nothing Then
            Session.Remove("TimecardAssistanceGrid")
        End If

        Session.Add("TimecardAssistanceGrid", NTSGeneralGridTC.DataSource)

        PunchDict = Nothing
        dt.Dispose()
        dp = Nothing

    End Sub

    ''' <summary >
    ''' Fills the top grid with the punch hours and departments.
    ''' </summary>
    ''' <param name="ScheduleList" >The employee schedule.</param>
    Public Sub PopulateScheduleGrid(ByVal ScheduleList As List(Of ATGetEmployeeScheduleByPeriod_Result))

        Dim dt As New DataTable
        Dim weekday As Integer
        Dim prevweekday As Integer = 0
        Dim ScheduleListMaxIndex As Integer = ScheduleList.Count - 1
        Dim punchmismatchbuffer As Integer = 0
        Dim maxmismatch As Integer = 35

        For Int As Integer = 1 To 7
            dt.Columns.Add(Int.ToString, GetType(String))
        Next

        Dim arow As DataRow = dt.NewRow

        While ScheduleList.Count <> 0

            For Int As Integer = 0 To ScheduleListMaxIndex Step 1
                weekday = ScheduleList(Int).DayOfWeek

                If weekday <> prevweekday Then

                    If prevweekday > ScheduleList(Int).DayOfWeek Then
                        dt.Rows.Add(arow)
                        arow = dt.NewRow()
                    End If

                    arow(ScheduleList(Int).DayOfWeek.ToString) = ScheduleList(Int).Hour
                    prevweekday = ScheduleList(Int).DayOfWeek
                    ScheduleList.Remove(ScheduleList(Int))
                    ScheduleListMaxIndex = ScheduleListMaxIndex - 1
                    Int = Int - 1
                    punchmismatchbuffer = 0

                Else

                    punchmismatchbuffer = punchmismatchbuffer + 1
                    If punchmismatchbuffer = maxmismatch Then
                        Exit While
                    End If

                End If

                If ScheduleListMaxIndex = Int Then
                    Exit For
                End If

            Next

        End While

        dt.Rows.Add(arow)

        If punchmismatchbuffer = maxmismatch Then
            For Each elemento As ATGetEmployeeScheduleByPeriod_Result In ScheduleList
                Dim somerow As DataRow = dt.NewRow
                somerow(elemento.DayOfWeek.ToString) = elemento.Hour.ToString
                dt.Rows.Add(somerow)
            Next
        End If

        NTSGridSchedule.DataSource = dt
        NTSGridSchedule.DataBind()

        If Not Session("ScheduleGrid") Is Nothing Then
            Session.Remove("ScheduleGrid")
        End If

        Session.Add("ScheduleGrid", NTSGridSchedule.DataSource)

    End Sub

    ''' <summary >
    ''' Refreshes the punchs grid.
    ''' </summary>
    Protected Sub NTSGeneralGridTC_NeedDataSource(sender As Object, e As Telerik.Web.UI.GridNeedDataSourceEventArgs) Handles NTSGeneralGridTC.NeedDataSource

        NTSGeneralGridTC.DataSource = Session("TimecardAssistanceGrid")

    End Sub

    ''' <summary >
    ''' Refreshes the schedule grid.
    ''' </summary>
    Protected Sub NTSGridSchedule_NeedDataSource(sender As Object, e As Telerik.Web.UI.GridNeedDataSourceEventArgs) Handles NTSGridSchedule.NeedDataSource

        NTSGridSchedule.DataSource = Session("ScheduleGrid")

    End Sub

    ''' <summary >
    ''' Refreshes the daily sums grid.
    ''' </summary>
    Protected Sub NTSGridHoursSum_NeedDataSource(sender As Object, e As Telerik.Web.UI.GridNeedDataSourceEventArgs) Handles NTSGridHoursSum.NeedDataSource

        NTSGridHoursSum.DataSource = Session("HourGrid")

    End Sub

    ''' <summary >
    ''' Refreshes the additional hours grid.
    ''' </summary>
    Protected Sub AdditionalGrid_NeedDataSource(sender As Object, e As Telerik.Web.UI.GridNeedDataSourceEventArgs) Handles AdditionalGrid.NeedDataSource

        AdditionalGrid.DataSource = Session("AdditionalHours")

    End Sub

    'Protected Sub lnkbtnRecalculate_Click(sender As Object, e As EventArgs) Handles lnkbtnRecalculate.Click
    '    If Integer.TryParse(Request.Params("ID"), TimeCardID) Then
    '        If ATC.Value = "" Then
    '            DoCalcs(TimeCardID)
    '        Else
    '            DoCalcs(Integer.Parse(ATC.Value))
    '        End If
    '    End If
    'End Sub

    ''' <summary >
    ''' Displays the next timecard if available.
    ''' </summary>
    Public Sub GoNext()
        Dim CurrentTimeCard As Integer = -1
        If Integer.TryParse(Request.Params("ID"), TimeCardID) Then
            If ATC.Value <> "" Then
                CurrentTimeCard = Integer.Parse(ATC.Value)
                If IsThereNextPeriod(CurrentTimeCard) Then
                    ATC.Value = CurrentTimeCard
                    DoCalcs(CurrentTimeCard)
                End If
            Else
                ATC.Value = TimeCardID
                CurrentTimeCard = TimeCardID
                If IsThereNextPeriod(CurrentTimeCard) Then
                    ATC.Value = CurrentTimeCard
                    DoCalcs(CurrentTimeCard)
                End If
            End If

        End If
    End Sub

    ''' <summary >
    ''' Displays the previous timecard if available
    ''' </summary>
    Public Sub GoPrevious()
        Dim CurrentTimeCard As Integer = -1
        If Integer.TryParse(Request.Params("ID"), TimeCardID) Then
            If ATC.Value <> "" Then
                CurrentTimeCard = Integer.Parse(ATC.Value)
                If IsTherePreviousPeriod(CurrentTimeCard) Then
                    ATC.Value = CurrentTimeCard
                    DoCalcs(CurrentTimeCard)
                End If
            Else
                ATC.Value = TimeCardID
                CurrentTimeCard = TimeCardID
                If IsTherePreviousPeriod(CurrentTimeCard) Then
                    ATC.Value = CurrentTimeCard
                    DoCalcs(CurrentTimeCard)
                End If
            End If

        End If
    End Sub

    ''' <summary >
    ''' This is a proxy method to the GoNext procedure.
    ''' </summary>
    Protected Sub LnkBtnNext_Click(sender As Object, e As EventArgs) Handles LnkBtnNext.Click
        GoNext()
    End Sub

    ''' <summary >
    ''' This is a proxy method to the GoPrevious procedure.
    ''' </summary>
    Protected Sub LnkBtnPrevious_Click(sender As Object, e As EventArgs) Handles LnkBtnPrevious.Click
        GoPrevious()
    End Sub

    ''' <summary >
    ''' This is a proxy method to the GoPrevious procedure.
    ''' </summary>
    Protected Sub lnkBtn_Previous_Click(sender As Object, e As EventArgs) Handles lnkBtn_Previous.Click
        GoPrevious()
    End Sub

    ''' <summary >
    ''' This is a proxy method to the GoNext procedure.
    ''' </summary>
    Protected Sub lnkbtn_NextPeriod_Click(sender As Object, e As EventArgs) Handles lnkbtn_NextPeriod.Click
        GoNext()
    End Sub

    ''' <summary >
    ''' Checks if there is another timecard for the next period.
    ''' </summary>
    ''' <param name="CurrentTimeCard">The ID of tblattimecards.ID</param>
    ''' <returns>True if there is a next timecard, false otherwise.</returns>
    Public Function IsThereNextPeriod(ByRef CurrentTimeCard As Integer) As Boolean
        Dim dp As New AttendanceDataProvider

        Dim IdList As List(Of Integer?) = dp.ATGetNextTimeCardID(Session("CompanyID"), CurrentTimeCard)
        If IdList.Count = 0 Then
            Return False
        Else
            CurrentTimeCard = IdList(0)
            Return True
        End If

        Return False
    End Function

    ''' <summary >
    ''' Validates the existance of a previous timecard.
    ''' Stored procedure: ATGetPreviousPeriodTC
    ''' </summary>
    ''' <param name="CurrentTimeCard" >ID of table tblattimecards.ID</param>
    Public Function IsTherePreviousPeriod(ByRef CurrentTimeCard As Integer) As Boolean

        Dim dp As New AttendanceDataProvider

        Dim IdList As List(Of Integer?) = dp.ATGetPreviousTimeCardID(Session("CompanyID"), CurrentTimeCard)
        If IdList.Count = 0 Then
            Return False
        Else
            CurrentTimeCard = IdList(0)
            Return True
        End If

        Return False

    End Function

    ''' <summary >
    ''' Validates the current timecard ID and redirects the user to the schedule detail page.
    ''' Stored procedure: ATGetEmployeeEntryNumByTimeCard
    ''' </summary>
    Protected Sub lnkBtnEdit_Click(sender As Object, e As EventArgs) Handles lnkBtnEdit.Click
        If Not Integer.TryParse(Request.Params("ID"), TimeCardID) Then
            'ID validation
            Return
        End If
        If ATC.Value.Trim <> "" Then
            TimeCardID = Integer.Parse(ATC.Value.Trim)
        End If
        Dim dp As New AttendanceDataProvider, EntryNum As Integer = -1
        Dim EntryNumList As List(Of Integer?) = dp.ATGetEmployeeEntryNumByTimeCardId(Session("CompanyID"), TimeCardID)
        If EntryNumList.Count > 0 Then
            EntryNum = EntryNumList(0).Value
        End If
        Response.Redirect("~\WebPages\Detail\Attendance\ATEmployeeScheduleDetail.aspx?action=edit&ID=" + EntryNum.ToString)
    End Sub

    ''' <summary>
    ''' Fills the grid with to total amount of hours per day.
    ''' </summary>
    ''' <param name="ScheduleList">The list of the week punchs.</param>
    ''' <remarks>Total amount of hours per day is an exact amount; rounding ignored.</remarks>
    Public Sub PopulateHoursSumGrid(ByVal ScheduleList As List(Of ATTimeCardPunchesByTCID_Result))

        Dim dt_toinsert As New DataTable, PunchList As New List(Of ATTimeCardPunchesByTCID_Result), arow As DataRow

        For Int As Integer = 1 To 7
            dt_toinsert.Columns.Add(Int.ToString, GetType(String))
        Next

        arow = dt_toinsert.NewRow

        For Int As Integer = 1 To 7

            PunchList.Clear()

            For a As Integer = 0 To ScheduleList.Count - 1 Step 1

                If ScheduleList(a).DayOfWeek = Int Then

                    PunchList.Add(ScheduleList(a))

                End If

            Next

            arow(Int.ToString) = Math.Round(SumDayHours(PunchList), 2, MidpointRounding.AwayFromZero)

        Next

        'Post total hours in grid
        dt_toinsert.Rows.Add(arow)
        NTSGridHoursSum.DataSource = dt_toinsert
        NTSGridHoursSum.DataBind()

        If Not Session("HourGrid") Is Nothing Then
            Session.Remove("HourGrid")
        End If

        Session.Add("HourGrid", NTSGridHoursSum.DataSource)

    End Sub

    ''' <summary>
    ''' Sums the amount of hours of a day.
    ''' </summary>
    ''' <param name="ScheduleList"></param>
    ''' <returns>The amount of hours in a day; not rounded.</returns>
    ''' <remarks></remarks>
    Public Function SumDayHours(ByRef ScheduleList As List(Of ATTimeCardPunchesByTCID_Result)) As Double

        Dim WeekHours As Double = 0
        Dim PunchInHour As TimeSpan = Nothing

        'Autopunch Commented No go
        ''Sum From 12Am if first punch is punch out
        If ScheduleList.Count > 0 And cbxOvernight.Checked Then
            If Not ScheduleList(0).IsPunchIn Then

                Dim autopuncham As New ATTimeCardPunchesByTCID_Result()
                autopuncham.Hour = New TimeSpan(0, 0, 0)
                autopuncham.ID = -1
                autopuncham.IsPunchIn = True
                autopuncham.PType = "Auto"
                ScheduleList.Insert(0, autopuncham)

            End If

            'Sum to 11:59pm if last punch is punch  in
            If ScheduleList(ScheduleList.Count - 1).IsPunchIn Then

                Dim autopunchpm As New ATTimeCardPunchesByTCID_Result()
                autopunchpm.Hour = New TimeSpan(11, 59, 59)
                autopunchpm.ID = -1
                autopunchpm.IsPunchIn = False
                autopunchpm.PType = "Auto"
                ScheduleList.Add(autopunchpm)

            End If

        End If

        For Each element As ATTimeCardPunchesByTCID_Result In ScheduleList

            If element.IsPunchIn Then
                PunchInHour = element.Hour
            Else

                If PunchInHour <> Nothing Then

                    If PunchInHour > element.Hour Then
                        WeekHours = WeekHours + ((New TimeSpan(23, 59, 59)).Subtract(PunchInHour).TotalHours + (New TimeSpan(0, 0, 0)).Add(element.Hour).TotalHours)
                    Else
                        WeekHours = WeekHours + CType(element.Hour, TimeSpan).Subtract(PunchInHour).TotalHours
                    End If

                    PunchInHour = Nothing
                Else

                    'missing punch in
                End If

            End If

        Next

        If ScheduleList.Count > 0 Then
            If Not ScheduleList(0).IsPunchIn Then
                WeekHours = WeekHours + ((New TimeSpan(23, 59, 59)).Subtract(ScheduleList(ScheduleList.Count - 1).Hour).TotalHours + _
                                         (New TimeSpan(0, 0, 0)).Add(ScheduleList(0).Hour).TotalHours)
            End If
        End If

        Return WeekHours

    End Function

    ''' <summary>
    ''' Sums the amount of holiday hours in a week.
    ''' </summary>
    ''' <param name="ScheduleList">The list of punchs in a week.</param>
    ''' <returns>The amount of holiday hours.</returns>
    ''' <remarks>Since the use of holiday and sick punchs, this function is not being used.</remarks>
    Public Function SumHolidayHours(ByRef ScheduleList As List(Of ATTimeCardPunchesByTCID_Result)) As Double
        Dim PunchList As New List(Of ATTimeCardPunchesByTCID_Result)
        Dim HolidayHours As Double = 0
        For Int As Integer = 1 To 7

            PunchList.Clear()

            For a As Integer = 0 To ScheduleList.Count - 1 Step 1

                If ScheduleList(a).DayOfWeek = Int And ScheduleList(a).PType = "Holiday" Then

                    PunchList.Add(ScheduleList(a))

                End If

            Next

            HolidayHours = HolidayHours + Math.Round(SumDayHours(PunchList), 2, MidpointRounding.AwayFromZero)

        Next

        Return HolidayHours
    End Function

    ''' <summary>
    ''' Makes a copy of object ATTimeCardPunchesByTCID 
    ''' </summary>
    ''' <param name="Source_Obj">The object to be copied.</param>
    ''' <param name="Dest_Obj">The copied object.</param>
    ''' <remarks></remarks>
    Public Sub Copy_Obj(ByVal Source_Obj As List(Of ATTimeCardPunchesByTCID_Result), ByVal Dest_Obj As List(Of ATTimeCardPunchesByTCID_Result))
        For Each element As ATTimeCardPunchesByTCID_Result In Source_Obj
            Dim anew As New ATTimeCardPunchesByTCID_Result
            anew.Approved = element.Approved
            anew.DayOfWeek = element.DayOfWeek
            anew.Hour = element.Hour
            anew.ID = element.ID
            anew.IsPunchIn = element.IsPunchIn
            anew.PType = element.PType
            anew.Edited = element.Edited
            anew.Dpt = element.Dpt
            Dest_Obj.Add(anew)
        Next
    End Sub

    ''' <summary>
    ''' Toggles the approval status of a timecard.
    ''' </summary>
    ''' <param name="sender">The approval checkbox object.</param>
    ''' <param name="e">The command arguments.</param>
    ''' <remarks>Stored procedures: ATApproveTimeCard, ATDisApproveTimeCard </remarks>
    Protected Sub cbxApproved_CheckedChanged(sender As Object, e As EventArgs)
        If Not Integer.TryParse(Request.Params("ID"), TimeCardID) Then
            'ID validation
            Return
        End If
        If ATC.Value.Trim <> "" Then
            TimeCardID = Integer.Parse(ATC.Value)
        End If
        Dim dp As New AttendanceDataProvider
        If cbxApproved.Checked Then
            dp.ATApproveTimeCard(Session("CompanyID"), TimeCardID, Session("UserGuidID"))
            lnkbtnAddPunch.Enabled = False
        Else
            dp.ATDisApproveTimeCard(Session("CompanyID"), TimeCardID)
            lnkbtnAddPunch.Enabled = True
        End If

        dp = Nothing
    End Sub

    ''' <summary>
    ''' Checks the approval status of a timecard.
    ''' </summary>
    ''' <returns>True if the timecard is approved, false otherwise.</returns>
    ''' <remarks>Stored procedure: ATGetTimeCardApproval </remarks>
    Public Function IsApproved() As Boolean
        If Not Integer.TryParse(Request.Params("ID"), TimeCardID) Then
            'ID validation
            Return False
        End If
        If ATC.Value.Trim <> "" Then
            TimeCardID = Integer.Parse(ATC.Value.Trim)
        End If
        Dim dp As New AttendanceDataProvider

        Return dp.ATTimeCardIsApproved(Session("CompanyID"), TimeCardID)
    End Function

    ''' <summary>
    ''' Deletes a manual hour amount entry.
    ''' </summary>
    ''' <param name="source">The delete link button.</param>
    ''' <param name="e">Delete event arguments</param>
    ''' <remarks>The import process looks for these entries during the import process; meanwhile these entries do not affect the registered
    ''' punch calculations. The manual hours entered is an alternate process of requesting a license through the request license page.
    ''' Stored procedures: ATGetWeekPayrollPeriodByTimeCardID , ATGetManualHours</remarks>
    Private Sub AdditionalGrid_DeleteCommand(ByVal [source] As Object, ByVal e As Telerik.Web.UI.GridCommandEventArgs) Handles AdditionalGrid.DeleteCommand
        Dim ID As String = e.Item.OwnerTableView.DataKeyValues(e.Item.ItemIndex)("ID").ToString()
        Dim pcclass As New ATPunchClock
        Dim table As DataTable = pcclass.ConvertToDataTable(Session("AdditionalHours"))
        table.PrimaryKey = New DataColumn() {table.Columns("ID")}
        If Not (table.Rows.Find(ID) Is Nothing) Then
            table.Rows.Find(ID).Delete()
            table.AcceptChanges()
        End If
        Try

            Dim CurrentTimeCard As Integer = -1
            If Integer.TryParse(Request.Params("ID"), TimeCardID) Then
                If ATC.Value <> "" Then
                    CurrentTimeCard = Integer.Parse(ATC.Value)
                Else
                    ATC.Value = TimeCardID
                    CurrentTimeCard = TimeCardID
                End If

            End If

            Dim dp As New AttendanceDataProvider

            dp.ATDeleteManualHoursEntry(Integer.Parse(ID))

            Dim PayrollPeriodList As List(Of ATGetWeekPayrollPeriodByTimeCardID_Result) = dp.ATGetTimeCardWeek(Session("CompanyID"), CurrentTimeCard)

            StartDate = PayrollPeriodList(0).WeekStart
            EndDate = PayrollPeriodList(0).WeekEnd

            PayrollPeriodList = Nothing

            'Get Additional Hours
            Dim AdditionalHoursList As List(Of ATGetManualHours_Result) = dp.ATGetManualHours(Session("CompanyID"), StartDate, EndDate, CurrentTimeCard)
            AdditionalGrid.DataSource = AdditionalHoursList
            AdditionalGrid.DataBind()

            If Not Session("AdditionalHours") Is Nothing Then
                Session.Remove("AdditionalHours")
            End If

            Session.Add("AdditionalHours", AdditionalGrid.DataSource)

            dp = Nothing
        Catch ex As Exception

        End Try
        pcclass.Dispose()
        table.Dispose()
    End Sub

    ''' <summary>
    ''' Manages the dropdown index change of employees.
    ''' </summary>
    ''' <param name="sender">The employee dropdownlist.</param>
    ''' <param name="e">The events arguments</param>
    ''' <remarks>Stored procedures: ATLookID, ATGetESSInfo, ATGetEmployeeInfoByUserSetup, ATIDMatch </remarks>
    Protected Sub ddlEmpList_SelectedIndexChanged(sender As Object, e As System.EventArgs)
        Dim dp As New AttendanceDataProvider

        Dim IsSuperRole As Boolean = Session("IsATAdmin")
        Dim IsSuperVisor As Boolean = Session("IsATSuperVisor")
        Integer.TryParse(ddlEmpList1.SelectedValue, TimeCardID)

        If Not (IsSuperRole Or IsSuperVisor) Then
            cbxApproved.Visible = False
            liApproved.Visible = False
            ddlEmpList1.Visible = False
            If Not dp.ATIDMatch(Session("CompanyID"), New Guid(Session("UserGuidID").ToString), TimeCardID) Then
                'check self service
                Dim ESSList As List(Of ATGetESSInfo_Result) = dp.ATGetEmpSSPInfo(Session("CompanyID"), New Guid(Session("UserGuidID").ToString))
                If ESSList.Count = 0 Then
                    'Check user setup
                    Dim UserSetupInfo As List(Of ATGetEmployeeInfoByUserSetup_Result) = dp.ATGetEmployeeInfoByUserSetup(Session("CompanyID"), New Guid(Session("UserGuidID").ToString))
                    If UserSetupInfo.Count > 0 Then
                        'check id match
                        If Not dp.ATIDMatchByEntryNum(Session("CompanyID"), UserSetupInfo(0).EntryNum, TimeCardID) Then
                            Response.Redirect("~/WebPages/General/Attendance/ATTimeCardsGeneral.aspx")
                        End If
                    Else
                        dp = Nothing
                        Response.Redirect("~/WebPages/General/Attendance/ATTimeCardsGeneral.aspx")
                    End If
                Else
                    'check id match
                    If Not dp.ATIDMatchByEntryNum(Session("CompanyID"), ESSList(0).EntryNum, TimeCardID) Then
                        Response.Redirect("~/WebPages/General/Attendance/ATTimeCardsGeneral.aspx")
                    End If
                End If

            End If
        Else

            'FillEmployeeDropDown(TimeCardID, IsSuperRole, IsSuperVisor, dp)

        End If

        dp = Nothing

        ATC.Value = TimeCardID
        DoCalcs(TimeCardID)
    End Sub

    Protected Sub lnkbtnAddHours_Click(sender As Object, e As EventArgs)
        Response.Redirect("~/WebPages/Detail/Attendance/ATLicenseRequest.aspx")
    End Sub

    Public Sub CheckTimecardVersion()
        If Request.Params("ID") Is Nothing Then
            'No timecard selected
            Response.Redirect("~/WebPages/General/Attendance/ATTimeCardsGeneral.aspx")
            Return
        End If
        If Not Integer.TryParse(Request.Params("ID"), TimeCardID) Then
            'ID validation
            Response.Redirect("~/WebPages/General/Attendance/ATTimeCardsGeneral.aspx")
            Return
        End If
        Dim dp As New AttendanceDataProvider
        'Check which version to use and redirect if V2 is used and periods are set for this TC  otherwise display popup warning.
        'Need a Q just to check versions and periods at the same time.
        Dim TimecardsSetup As List(Of ATCheckTCVersionAndPeriods_Result) = dp.ATCheckTCVersionAndPeriods(Session("CompanyID"), TimeCardID)
        If TimecardsSetup.Count > 0 Then
            'If TimecardsSetup(0).EmployeeFrequency <> 52 Then
            If (Not TimecardsSetup(0).isPeriodSetForTC) And TimecardsSetup(0).UseTCVersion = 2 Then
                'Pop UP
                Warn.Value = "1"
            Else
                If TimecardsSetup(0).UseTCVersion = 2 Then
                    Response.Redirect("~/WebPages/Detail/Attendance/ATTimeCardDtl.aspx?action=view&ID=" & TimeCardID.ToString)
                End If
            End If

            'End If
        End If

        dp = Nothing
    End Sub

    Protected Overrides Sub InitializeCulture()
        Dim Culture As String = Convert.ToString(Session("Language"))
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Culture)
        Thread.CurrentThread.CurrentUICulture = New CultureInfo(Culture)
        MyBase.InitializeCulture()
    End Sub

End Class