Imports System.Threading
Imports NLog
Imports System.Text

''' <summary>
''' A helper class that makes it easy to run methods in an asynchronous
''' fire-and-forget manner without blocking the UI thread.
''' </summary>
''' <remarks>
''' <para>
''' Created By:  Bryan Johns<br />
''' On:  1/27/2011 at 1:51 PM
''' </para>
''' <para>
''' Translated from a C# class found at:
''' http://www.eggheadcafe.com/articles/20050818.asp
''' </para>
''' </remarks>
Public Class AsyncHelper

    ''' <summary>
    ''' A class to hold the target method delegate and the argument
    ''' array to be passed to the call back method.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Created By:  Bryan Johns<br />
    ''' On:  1/27/2011 at 2:10 PM
    ''' </para>
    ''' </remarks>
    Private Class TargetInfo
        Friend Sub New(ByVal d As [Delegate], ByVal arguments As Object())
            Target = d
            Args = arguments
        End Sub

        Friend ReadOnly Target As [Delegate]
        Friend ReadOnly Args As Object()
    End Class

    Private Shared ReadOnly dynamicInvokeShimCallback As New WaitCallback(AddressOf DynamicInvokeShim)

    ''' <summary>
    ''' This is the entry point to the class.  It takes a delegate pointing to the
    ''' method to be executed in the background thread.
    ''' </summary>
    ''' <param name="d">A delegate for the target method to be executed.</param>
    ''' <param name="args">An array of objects holding the parameters of the target method.</param>
    ''' <remarks>
    ''' <para>
    ''' Created By:  Bryan Johns<br />
    ''' On:  1/27/2011 at 1:56 PM
    ''' </para>
    ''' <example>
    ''' This example shows how to call the <see cref="AsyncHelper.FireAndForget" /> method.
    ''' <code>AsyncHelper.FireAndForget(delegate,param1,param2)</code>
    ''' </example>
    ''' </remarks>
    Public Shared Sub FireAndForget(ByVal d As [Delegate], ByVal ParamArray args As Object())
        ThreadPool.QueueUserWorkItem(dynamicInvokeShimCallback, New TargetInfo(d, args))
    End Sub

    ''' <summary>
    ''' A callback method to catch and log any exceptions to the
    ''' <c>system.trace.diagnostics</c> trace because it's a thread-safe
    ''' way to log exceptions.
    ''' </summary>
    ''' <param name="o">The the target method that will call back.</param>
    ''' <remarks>
    ''' <para>
    ''' Created By:  Bryan Johns<br />
    ''' On:  1/27/2011 at 2:07 PM
    ''' </para>
    ''' <para>
    ''' 3/11/2011 at a quarter till eleven AM.
    ''' This class now depends on NLog to handle reporting of exceptions that occur
    ''' in the background thread.  Since this is an implementation of the "Fire and Forget"
    ''' pattern, a thread-safe method of reporting exceptions is required.  The best way
    ''' to accomplish this, especially in a production web environment, is via <see cref="NLog" /> or a
    ''' similar tread-safe library.  The <see cref="System.Diagnostics.EventLogTraceListener" />
    ''' would work but it requires extra setup and permissions to record to the event log.
    ''' NLog is a more reliable and flexible system since it can log to a database, email,
    ''' or any number of other targets.
    ''' </para>
    ''' </remarks>
    Private Shared Sub DynamicInvokeShim(ByVal o As Object)
        Try
            Dim ti As TargetInfo = DirectCast(o, TargetInfo)
            ti.Target.DynamicInvoke(ti.Args)
        Catch ex As Exception
            ' log to the thread-safe NLog

            Dim logger As Logger = LogManager.GetCurrentClassLogger
            logger.ErrorException(GetFormattedException(ex), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Gets the formatted exception.
    ''' </summary>
    ''' <param name="ex">The <see cref="System.Exception" /> to be formated.</param>
    ''' <param name="isRecursive">if set to <c>true</c> [is recursive].</param>
    ''' <returns>A formated representation of the exception including any inner exceptions.</returns>
    ''' <remarks>
    ''' <para>
    ''' This function recursively iterates through an exception and all of its innerexceptions to
    ''' get a formated version of all of it's messages.
    ''' </para>
    ''' <para>
    ''' Created By:  Bryan Johns<br />
    ''' On:  3/11/2011 at 11:10 AM
    ''' </para>
    ''' </remarks>
    Private Shared Function GetFormattedException(ByRef ex As Exception,
     Optional ByVal isRecursive As Boolean = False) As String
        Dim sb As New StringBuilder
        If isRecursive Then
            sb.AppendFormat("*********************** Inner Exception ************************{0}", Environment.NewLine)
        Else
            sb.AppendFormat("************************* Exception ***************************{0}", Environment.NewLine)
        End If
        sb.AppendFormat("Exception Message:{0}{1}{2}", Environment.NewLine, ex.Message, Environment.NewLine)
        sb.AppendFormat("Stack Trace:{0}{1}{2}", Environment.NewLine, ex.StackTrace, Environment.NewLine)
        sb.AppendFormat("Source:{0}{1}{2}", Environment.NewLine, ex.Source, Environment.NewLine)
        ' recurse into inner exceptions
        If ex.InnerException IsNot Nothing Then
            sb.AppendFormat("{0}{1}", GetFormattedException(ex.InnerException, True), Environment.NewLine)
        End If
        Dim msg As String = sb.ToString
        Return msg
    End Function

End Class
