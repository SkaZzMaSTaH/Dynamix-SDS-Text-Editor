/* This code belongs to Rod Stephens, publishing it on March 14, 2018
 * on his blog (http://csharphelper.com/blog/2018/03/get-assembly-information-in-c/). */

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;

namespace Manager
{
    public class AssemblyInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Company { get; set; }
        public string Product { get; set; }
        public string Copyright { get; set; }
        public string Trademark { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public string Guid { get; set; }
        public string NeutralLanguage { get; set; }
        public bool IsCOMVisible { get; set; }

        public AssemblyInfo()
            : this(Assembly.GetExecutingAssembly())
        {

        }

        public AssemblyInfo(Assembly assembly)
        {
            // Get values from the assembly.
            AssemblyTitleAttribute titleAttr =
                GetAssemblyAttribute<AssemblyTitleAttribute>(assembly);
            if (titleAttr != null) Title = titleAttr.Title;

            AssemblyDescriptionAttribute assemblyAttr =
                GetAssemblyAttribute<AssemblyDescriptionAttribute>(assembly);
            if (assemblyAttr != null) Description =
                assemblyAttr.Description;

            AssemblyCompanyAttribute companyAttr =
                GetAssemblyAttribute<AssemblyCompanyAttribute>(assembly);
            if (companyAttr != null) Company = companyAttr.Company;

            AssemblyProductAttribute productAttr =
                GetAssemblyAttribute<AssemblyProductAttribute>(assembly);
            if (productAttr != null) Product = productAttr.Product;

            AssemblyCopyrightAttribute copyrightAttr =
                GetAssemblyAttribute<AssemblyCopyrightAttribute>(assembly);
            if (copyrightAttr != null) Copyright = copyrightAttr.Copyright;

            AssemblyTrademarkAttribute trademarkAttr =
                GetAssemblyAttribute<AssemblyTrademarkAttribute>(assembly);
            if (trademarkAttr != null) Trademark = trademarkAttr.Trademark;

            AssemblyVersion = assembly.GetName().Version.ToString();

            AssemblyFileVersionAttribute fileVersionAttr =
                GetAssemblyAttribute<AssemblyFileVersionAttribute>(assembly);
            if (fileVersionAttr != null) FileVersion =
                fileVersionAttr.Version;

            GuidAttribute guidAttr = GetAssemblyAttribute<GuidAttribute>(assembly);
            if (guidAttr != null) Guid = guidAttr.Value;

            NeutralResourcesLanguageAttribute languageAttr =
                GetAssemblyAttribute<NeutralResourcesLanguageAttribute>(assembly);
            if (languageAttr != null) NeutralLanguage =
                languageAttr.CultureName;

            ComVisibleAttribute comAttr =
                GetAssemblyAttribute<ComVisibleAttribute>(assembly);
            if (comAttr != null) IsCOMVisible = comAttr.Value;
        }

        public static T GetAssemblyAttribute<T>(Assembly assembly)
            where T : Attribute
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(T), true);

            if ((attributes == null) || attributes.Length == 0) { return null; }

            return (T)attributes[0];
        }
    }
}
