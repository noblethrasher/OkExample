Imports System.IO
Imports System.Xml
Imports System.Xml.Xsl

Public Class AdviseeEmail

    Dim HTMLView As String

    Shared ReadOnly StyleSheet =
        <?xml version="1.0"?>

        <stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

            <xsl:template match="/">
                <xsl:copy>
                    <xsl:apply-templates/>
                </xsl:copy>
            </xsl:template>


            <xsl:template match="a">
                <a>
                    <xsl:attribute name="href">
                        <value-of select="href"/>
                    </xsl:attribute>
                    <xsl:value-of select="text"/>
                </a>
            </xsl:template>


        </stylesheet>.CreateReader()


    ReadOnly _ms As MemoryStream

    Public Sub New(advisee_name As String, advising_date As Date, canelation_token As Guid)


        Dim xml =
            <?xml version="1.0"?>
            <div>
                <p>Dear <%= advisee_name %>,</p>

                <p>Thank you for scheduling an appointment with the Study Abroad/ NSE Office on <%= advising_date.ToString("MMMM dd yyyy at h:mm tt") %></p>

                <p>If you need to cancel your appointment, you can do so by <a>
                        <href>http://abraod.okstate.edu/appointment/cancel/<%= canelation_token %></href>
                        <text>clicking here</text>
                    </a>.
                                    </p>
            </div>.CreateReader()


        _ms = New MemoryStream()
        Dim html = XmlWriter.Create(_ms)


        Dim xslt = New XslCompiledTransform()

        xslt.Load(StyleSheet)
        xslt.Transform(xml, html)

        _ms.Position = 0

    End Sub

    Public Overrides Function ToString() As String

        Return New StreamReader(_ms).ReadToEnd()

    End Function



End Class
