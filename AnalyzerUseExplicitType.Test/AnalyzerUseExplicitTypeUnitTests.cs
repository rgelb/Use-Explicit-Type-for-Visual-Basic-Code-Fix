using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = AnalyzerUseExplicitType.Test.CSharpCodeFixVerifier<
    AnalyzerUseExplicitType.AnalyzerUseExplicitTypeAnalyzer,
    AnalyzerUseExplicitType.AnalyzerUseExplicitTypeCodeFixProvider>;

using VerifyVB = AnalyzerUseExplicitType.Test.VisualBasicCodeFixVerifier<
    AnalyzerUseExplicitType.AnalyzerUseExplicitTypeAnalyzer,
    AnalyzerUseExplicitType.AnalyzerUseExplicitTypeCodeFixProvider>;



namespace AnalyzerUseExplicitType.Test
{
    [TestClass]
    public class AnalyzerUseExplicitTypeUnitTest
    {
        [TestMethod]
        public async Task TestMethod0() {
            var test = @"Module Module1
    Sub Main()
        Dim emp1 = New Employee()
        Dim emp2 As Employee = New Employee()
    End Sub
End Module

Public Class Employee
    Public Property FirstName As String
    Public Property LastName As String
End Class";

            await VerifyVB.VerifyAnalyzerAsync(test);
        }


        [TestMethod]
        public async Task TestMethod1() {
            var test = @"Module Module1
    Sub Main()
        Dim emp1 as New Employee()
        Dim emp2 As Employee = New Employee()
    End Sub
End Module

Public Class Employee
    Public Property FirstName As String
    Public Property LastName As String
End Class";

            await VerifyVB.VerifyAnalyzerAsync(test);
        }





        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethodFIX2() {
            var test = @"Module Module1
    Sub Main()
        Dim emp1 = New Employee()
        Dim emp2 As Employee = New Employee()
    End Sub
End Module

Public Class Employee
    Public Property FirstName As String
    Public Property LastName As String
End Class";

            var fixtest = @"Module Module1
    Sub Main()
        Dim emp1 as Employee = New Employee()
        Dim emp2 As Employee = New Employee()
    End Sub
End Module

Public Class Employee
    Public Property FirstName As String
    Public Property LastName As String
End Class"; 



            var expected = VerifyVB.Diagnostic("AnalyzerUseExplicitType").WithLocation(0).WithArguments("TypeName");
            await VerifyVB.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
