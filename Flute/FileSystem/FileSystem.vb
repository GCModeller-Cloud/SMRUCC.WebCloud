﻿Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports FolderHandle = Microsoft.VisualBasic.FileIO.Directory

Namespace FileSystem

    ''' <summary>
    ''' Physical file system combine with logical file mapping 
    ''' </summary>
    Public Class FileSystem

        Public ReadOnly Property wwwroot As FolderHandle

        ReadOnly virtualMaps As New Dictionary(Of String, FileObject)

        ''' <summary>
        ''' Create a new filesystem proxy for http web services
        ''' </summary>
        ''' <param name="wwwroot"></param>
        Sub New(wwwroot As String)
            Me.wwwroot = New FolderHandle(directory:=wwwroot)
        End Sub

        ''' <summary>
        ''' 这个函数只适用于小文件的缓存
        ''' </summary>
        ''' <param name="resourceUrl$"></param>
        ''' <param name="file$"></param>
        ''' <param name="mime"></param>
        ''' <returns></returns>
        Public Function AddCache(resourceUrl$, file$, Optional mime As Microsoft.VisualBasic.Net.Protocols.ContentTypes.ContentType = Nothing) As FileObject
            Return AddCache(resourceUrl, file.ReadBinary, mime)
        End Function

        Public Function AddCache(resourceUrl$, data As Byte(), Optional mime As Microsoft.VisualBasic.Net.Protocols.ContentTypes.ContentType = Nothing) As FileObject
            Dim resource As New MemoryCachedFile(resourceUrl.FileName, data, mime)
            Dim key$ = FileSystem.resourceUrl(resourceUrl)

            ' add new cache resource or update current 
            ' existed resource
            virtualMaps(key) = resource

            Return resource
        End Function

        Public Function AddMapping(resourceUrl$, file$, Optional mime As Microsoft.VisualBasic.Net.Protocols.ContentTypes.ContentType = Nothing) As FileObject
            Dim resource As New VirtualMappedFile(resourceUrl.FileName, file, mime)
            Dim key$ = FileSystem.resourceUrl(resourceUrl)

            ' add new cache resource or update current 
            ' existed resource
            virtualMaps(key) = resource

            Return resource
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="directory"></param>
        ''' <param name="attachTo"></param>
        ''' <param name="cacheMode">Work in cache mode or mapping mode?</param>
        ''' <returns></returns>
        Public Iterator Function AttachFolder(directory$, Optional attachTo$ = "/", Optional cacheMode As Boolean = False) As IEnumerable(Of NamedValue(Of FileObject))
            Dim resourceUrl$
            Dim fileObj As FileObject

            For Each file As String In ls - l - r - "*.*" <= directory
                resourceUrl = attachTo & "/" & RelativePath(directory, file)

                If cacheMode Then
                    fileObj = AddCache(resourceUrl, file)
                Else
                    fileObj = AddMapping(resourceUrl, file)
                End If

                Yield New NamedValue(Of FileObject) With {
                    .Name = resourceUrl,
                    .Description = file,
                    .Value = fileObj
                }
            Next
        End Function

        Private Shared Function resourceUrl(ByRef pathRelative As String) As String
            pathRelative = pathRelative.Trim("."c, "/"c, "\"c)
            Return pathRelative
        End Function

        Public Function GetResource(pathRelative As String) As Stream
            ' test of the physical file at first
            If resourceUrl(pathRelative).FileExists Then
                Return pathRelative.Open(FileMode.Open, doClear:=False)
            Else
                ' and then test for the logical file
                If virtualMaps.ContainsKey(pathRelative) Then
                    Return virtualMaps(pathRelative).GetResource
                End If
            End If

            Return New MemoryStream(buffer:={})
        End Function

        Public Function FileExists(pathRelative As String) As Boolean
            ' test of the physical file at first
            If resourceUrl(pathRelative).FileExists Then
                Return True
            Else
                ' and then test for the logical file
                If virtualMaps.ContainsKey(pathRelative) Then
                    If TypeOf virtualMaps(pathRelative) Is VirtualMappedFile Then
                        Return DirectCast(virtualMaps(pathRelative), VirtualMappedFile).isValid
                    Else
                        Return True
                    End If
                End If
            End If

            Return False
        End Function

        Public Overrides Function ToString() As String
            Return wwwroot.ToString
        End Function
    End Class
End Namespace