﻿Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.REST.AppEngine.APIMethods

Namespace AppEngine

    ''' <summary>
    ''' 执行<see cref="WebApp"/>的工作引擎
    ''' </summary>
    Public Class APPEngine

        Public ReadOnly Property [Namespace] As [Namespace]
        ''' <summary>
        ''' 必须按照从长到短来排序
        ''' </summary>
        Dim API As Dictionary(Of String, __API_Invoker)
        Public ReadOnly Property Application As Object

        Public Function GetHelp() As String
            Dim LQuery = (From api In Me.API Select "<li>" & api.Value.Help & "</li>").ToArray
            Dim result As String = String.Join("<br />" & vbCrLf, LQuery)
            Return result
        End Function

        Protected Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="api">已经变小写了的</param>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        Public Function Invoke(api As String, parameters As String, ByRef result As String) As Boolean
            If Not Me.API.ContainsKey(api) Then
                Return False
            End If

            Dim script As __API_Invoker = Me.API(api)
            Dim success As Boolean = script.Invoke(Application, parameters, result)

            Return success
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="api">已经变小写了的</param>
        ''' <param name="inputs"></param>
        ''' <returns></returns>
        Public Function Invoke(api As String, inputs As MemoryStream, ByRef result As String) As Boolean
            If Not Me.API.ContainsKey(api) Then
                Return False
            End If

            Dim script As __API_Invoker = Me.API(api)
            '   Dim success As Boolean = script.InvokePOST(Application, api, inputs, result)

            '  Return success
        End Function

        Public Shared Function InvokePOST(url As String, inputs As MemoryStream, applications As Dictionary(Of String, APPEngine), ByRef result As String) As Boolean
            Dim application As String = "", api As String = "", parameters As String = ""
            If Not APPEngine.GetParameter(url, application, api, parameters) Then
                Return False
            End If

            If Not applications.ContainsKey(application) Then
                Return False
            End If

            Return applications(application).Invoke(api, inputs, result)
        End Function

        Public Shared Function Invoke(url As String, applications As Dictionary(Of String, APPEngine), ByRef result As String) As Boolean
            Dim application As String = "", api As String = "", parameters As String = ""
            If Not APPEngine.GetParameter(url, application, api, parameters) Then
                Return False
            End If

            If Not applications.ContainsKey(application) Then
                Return False
            End If

            Return applications(application).Invoke(api, parameters, result)
        End Function

        Public Shared Function [Imports](Of T As WebApp)(obj As T) As APPEngine
            Dim Type As Type = obj.GetType
            Dim [namespace] = Type.NamespaceEntry

            If [namespace] Is Nothing OrElse [namespace].AutoExtract = True Then
                Call ServicesLogs.WriteEntry($"Could not found application entry point from {Type.FullName}".__DEBUG_ECHO,
                                             MethodBase.GetCurrentMethod,
                                             EventLogEntryType.FailureAudit)
                Return Nothing
            End If

            Dim Methods = Type.GetMethods(BindingFlags.Public Or BindingFlags.Instance)
            Dim EntryType As Type = CommandLine.Reflection.ExportAPIAttribute.TypeInfo
            Dim LQuery = (From EntryPoint As MethodInfo In Methods
                          Let attrs As Object() = EntryPoint.GetCustomAttributes(attributeType:=EntryType, inherit:=True)
                          Where Not attrs.IsNullOrEmpty
                          Let API = DirectCast(attrs(Scan0), CommandLine.Reflection.ExportAPIAttribute)                                         ' 由于rest服务需要返回json、所以在API的申明的时候还需要同时申明GET、POST里面所返回的json对象的类型，
                          Let httpMethod As APIMethod = DirectCast(EntryPoint.GetCustomAttributes(GetType(APIMethod), True)(Scan0), APIMethod)  ' 假若程序是在这里出错的话，则说明有API函数没有进行GET、POST的json类型申明，找到该函数补全即可
                          Let invoke = New __API_Invoker With {
                              .Name = API.Name.ToLower,
                              .EntryPoint = EntryPoint,
                              .Help = API.PrintView(HTML:=True) & $"<br /><div>{httpMethod.GetMethodHelp(EntryPoint)}</div>",
                              .Error404 = obj.Page404
                          }
                          Select invoke
                          Order By Len(invoke.Name) Descending).ToArray

            Return New APPEngine With {
                .API = LQuery.ToDictionary(Function(api) api.Name.ToLower),
                ._Application = obj,
                ._Namespace = [namespace]
            }
        End Function

        ''' <summary>
        ''' 返回False标识无法正确的解析出调用数据
        ''' </summary>
        ''' <param name="url"></param>
        ''' <param name="application"></param>
        ''' <param name="API"></param>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        Public Shared Function GetParameter(url As String, ByRef application As String, ByRef API As String, ByRef parameters As String) As Boolean
            Dim p As Integer = InStr(url, "?")
            Dim Tokens As String() = url.Split("/"c).Skip(1).ToArray

            If Tokens.IsNullOrEmpty Then
                Return False
            End If

            application = Tokens(Scan0)

            If p > 0 Then '带有参数
                API = Mid(url, 1, p - 1) '/application/function
                parameters = Mid(url, p + 1)

                If Tokens.Count = 1 Then
                    application = application.Split("?"c).First
                End If
            Else
                API = url
            End If

            application = application.ToLower
            If Not String.IsNullOrEmpty(API) Then
                API = API.ToLower
            End If

            Return True
        End Function
    End Class
End Namespace