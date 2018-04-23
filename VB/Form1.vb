Imports Microsoft.VisualBasic
Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports DevExpress.XtraRichEdit.Commands
Imports DevExpress.XtraRichEdit.API.Native

Namespace RichEditTOCGeneration
	Partial Public Class Form1
		Inherits Form
		Public ReadOnly Property Document() As Document
			Get
				Return richEditControl1.Document
			End Get
		End Property
		Private Delegate Sub TOCEntryFound(ByVal location As DocumentPosition, ByVal level As Integer)

		Public Sub New()
			InitializeComponent()
			richEditControl1.Options.Hyperlinks.ModifierKeys = Keys.None
			btnLoadTemplate_Click(btnLoadTemplate, EventArgs.Empty)
		End Sub

		Private Sub btnLoadTemplate_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnLoadTemplate.Click
			richEditControl1.LoadDocument("Employees.rtf")
		End Sub

		Private Sub btnShowAllFieldCodes_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShowAllFieldCodes.Click
			CType(New ShowAllFieldCodesCommand(richEditControl1), ShowAllFieldCodesCommand).Execute()
		End Sub

		Private Sub btnStyles_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnStyles.Click
			Document.BeginUpdate()
			ApplyStyles()
			InsertTOC("\h", True)
			Document.EndUpdate()
		End Sub

		Private Sub btnOutlineLevels_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnOutlineLevels.Click
			Document.BeginUpdate()
			AssignOutlineLevels()
			InsertTOC("\h \u", True)
			Document.EndUpdate()
		End Sub

		Private Sub btnTCFields_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnTCFields.Click
			Document.BeginUpdate()
			AddTCFields()
			InsertTOC("\h \f defaultGroup", True)
			Document.Fields.Update()
			Document.EndUpdate()
		End Sub

		Private Sub ApplyStyles()
			SearchForTOCEntries(Function(location, level) AnonymousMethod1(location, level))
		End Sub
		
		Private Function AnonymousMethod1(ByVal location As DocumentPosition, ByVal level As Integer) As Boolean
			Document.GetParagraph(location).Style = GetStyleForLevel(level)
			Return True
		End Function

		Private Sub AssignOutlineLevels()
			SearchForTOCEntries(Function(location, level) AnonymousMethod2(location, level))
		End Sub
		
		Private Function AnonymousMethod2(ByVal location As DocumentPosition, ByVal level As Integer) As Boolean
			Document.GetParagraph(location).OutlineLevel = level
			Return True
		End Function

		Private Sub AddTCFields()
			SearchForTOCEntries(Function(location, level) AnonymousMethod3(location, level))
		End Sub
		
		Private Function AnonymousMethod3(ByVal location As DocumentPosition, ByVal level As Integer) As Boolean
			Document.Fields.Add(location, String.Format("TC ""{0}"" \f {1} \l {2}", Document.GetText(Document.GetParagraph(location).Range), "defaultGroup", level))
			Return True
		End Function

		Private Sub SearchForTOCEntries(ByVal callback As TOCEntryFound)
			For i As Integer = 0 To Document.Paragraphs.Count - 1
				Dim range As DocumentRange = Document.CreateRange(Document.Paragraphs(i).Range.Start, 1)
				Dim cp As CharacterProperties = Document.BeginUpdateCharacters(range)
				Dim level As Integer = 0

				If cp.FontSize.Equals(14f) Then
					level = 1
				End If
				If cp.FontSize.Equals(13f) Then
					level = 2
				End If
				If cp.FontSize.Equals(11f) Then
					level = 3
				End If

				Document.EndUpdateCharacters(cp)

				If level <> 0 Then
					callback(range.Start, level)
				End If
			Next i
		End Sub

		Private Sub InsertTOC(ByVal switches As String, ByVal insertHeading As Boolean)
			If insertHeading Then
				InsertContentHeading()
			End If

			Dim field As Field = Document.Fields.Add(Document.Paragraphs((If(insertHeading, 1, 0))).Range.Start, "TOC " & switches)
			Dim cp As CharacterProperties = Document.BeginUpdateCharacters(field.Range)
			cp.Bold = False
			cp.FontSize = 12
			cp.ForeColor = Color.Blue
			Document.EndUpdateCharacters(cp)
			Document.InsertSection(field.Range.End)
			field.Update()
		End Sub

		Private Sub InsertContentHeading()
			Dim range As DocumentRange = Document.InsertText(Document.Range.Start, "Contents" & Constants.vbCrLf)
			Dim cp As CharacterProperties = Document.BeginUpdateCharacters(range)
			cp.FontSize = 18
			cp.ForeColor = Color.DarkBlue
			Document.EndUpdateCharacters(cp)
			Document.Paragraphs(0).Alignment = ParagraphAlignment.Center
			Document.Paragraphs(0).Style = Document.ParagraphStyles("Normal")
			Document.Paragraphs(0).OutlineLevel = 0
		End Sub

		Private Function GetStyleForLevel(ByVal level As Integer) As ParagraphStyle
			Dim styleName As String = "Paragraph Level " & level.ToString()
			Dim paragraphStyle As ParagraphStyle = Document.ParagraphStyles(styleName)

			If paragraphStyle Is Nothing Then
				paragraphStyle = Document.ParagraphStyles.CreateNew()
				paragraphStyle.Name = styleName
				paragraphStyle.Parent = Document.ParagraphStyles("Normal")
				paragraphStyle.OutlineLevel = level
				Document.ParagraphStyles.Add(paragraphStyle)
			End If

			Return paragraphStyle
		End Function
	End Class
End Namespace