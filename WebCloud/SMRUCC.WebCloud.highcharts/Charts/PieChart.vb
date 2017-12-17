﻿Namespace PieChart

    Public Class PieChart : Inherits Highcharts(Of serial)

        Public Overrides Function ToString() As String
            Return title.ToString
        End Function
    End Class

    Public Class PieChart3D : Inherits Highcharts3D(Of serial)

    End Class

    Public Class pieOptions : Inherits seriesoptions
        Public Property allowPointSelect As Boolean
        Public Property cursor As String
        Public Property depth As String
        Public Property dataLabels As dataLabels
        Public Property showInLegend As Boolean
    End Class

    Public Class pieData
        Public Property name As String
        Public Property y As Double
        Public Property sliced As Boolean
        Public Property selected As Boolean
    End Class

    Public Class serial : Inherits AbstractSerial(Of Object)

        ''' <summary>
        ''' + <see cref="Double"/>
        ''' + <see cref="pieData"/>
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Property data As Object()
    End Class
End Namespace