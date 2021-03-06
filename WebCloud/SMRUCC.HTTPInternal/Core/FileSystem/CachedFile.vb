﻿#Region "Microsoft.VisualBasic::704f00130d8d80e1c23910a93b940900, WebCloud\SMRUCC.HTTPInternal\Core\FileSystem\CachedFile.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class CachedFile
    ' 
    '         Properties: bufs, content, Path
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CacheAllFiles, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

Namespace Core.Cache

    Public Class CachedFile

        ''' <summary>
        ''' 文件路径
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Path As String
        ''' <summary>
        ''' 文件的内容
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property bufs As Byte()
        ''' <summary>
        ''' 文件的类型
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property content As ContentType

        Sub New(path As String)
            Me.Path = FileIO.FileSystem.GetFileInfo(path).FullName
            Me.bufs = File.ReadAllBytes(Me.Path)
            Me.Path = Me.Path.ToLower

            Dim ext As String = "." & Me.Path.Split("."c).Last

            Me.content = If(
                MIME.SuffixTable.ContainsKey(ext),
                MIME.SuffixTable(ext),
                MIME.UnknownType)
        End Sub

        Public Overrides Function ToString() As String
            Return $"({content.ToString}) {Path}"
        End Function

        ''' <summary>
        ''' 假若文件更新不频繁，可以在初始化阶段一次性的读取所有文件到内存中
        ''' </summary>
        ''' <param name="wwwroot"></param>
        ''' <returns></returns>
        Public Shared Function CacheAllFiles(wwwroot As String) As VirtualFileSystem
            Dim allFiles As IEnumerable(Of String) = FileIO.FileSystem.GetFiles(
                wwwroot,
                FileIO.SearchOption.SearchAllSubDirectories,
                "*.*")
            Dim hash As New VirtualFileSystem(False, New DirectoryInfo(wwwroot))

            For Each file As String In allFiles
                hash.Add(file, New CachedFile(file))
            Next

            Return hash
        End Function
    End Class
End Namespace
