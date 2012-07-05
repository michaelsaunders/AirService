﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 10.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace AirService.Web.Content.EmailTemplates
{
    using System;
    
    
    #line 1 "C:\Projects\Webling\AirService\AirService.Web\Content\EmailTemplates\SubscriptionCancelEmail.tt"
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public partial class SubscriptionCancelEmail : SubscriptionCancelEmailBase
    {
        public virtual string TransformText()
        {
            this.Write(@"
<html>
<body>
<p>We're sorry to see you go</p>
<p>Thanks for your time with AirService, we're sorry to lose you.</p>
<p>If you have any suggestions for ways we could improve our service please email us airservice@luxedigital.net, we'd love the chance to win you back.</p>
<br/>
Venue Details<br/>
Name: ");
            
            #line 16 "C:\Projects\Webling\AirService\AirService.Web\Content\EmailTemplates\SubscriptionCancelEmail.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(VenueName));
            
            #line default
            #line hidden
            this.Write("<br/>\r\nAddress: ");
            
            #line 17 "C:\Projects\Webling\AirService\AirService.Web\Content\EmailTemplates\SubscriptionCancelEmail.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Address));
            
            #line default
            #line hidden
            this.Write("<br/>\r\nPhone Number: ");
            
            #line 18 "C:\Projects\Webling\AirService\AirService.Web\Content\EmailTemplates\SubscriptionCancelEmail.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(PhoneNumber));
            
            #line default
            #line hidden
            this.Write("<br/>\r\nEmail Address: ");
            
            #line 19 "C:\Projects\Webling\AirService\AirService.Web\Content\EmailTemplates\SubscriptionCancelEmail.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Email));
            
            #line default
            #line hidden
            this.Write("<br/>\r\nType of Venue: ");
            
            #line 20 "C:\Projects\Webling\AirService\AirService.Web\Content\EmailTemplates\SubscriptionCancelEmail.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(TypeOfVenue));
            
            #line default
            #line hidden
            this.Write("<br/>\r\nReferral Code (if any): ");
            
            #line 21 "C:\Projects\Webling\AirService\AirService.Web\Content\EmailTemplates\SubscriptionCancelEmail.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(ReferralCode));
            
            #line default
            #line hidden
            this.Write("<br/>\r\n\r\n<p>\r\nKind regards<br/>\r\nThe AirService Team<br/>\r\n<br/>\r\nLuxe Digital Pt" +
                    "y Ltd ABN 30 255 155 711\r\n</p>\r\n</body>\r\n</html>\r\n\r\n");
            return this.GenerationEnvironment.ToString();
        }
        
        #line 1 "C:\Projects\Webling\AirService\AirService.Web\Content\EmailTemplates\SubscriptionCancelEmail.tt"

private string _VenueNameField;

/// <summary>
/// Access the VenueName parameter of the template.
/// </summary>
private string VenueName
{
    get
    {
        return this._VenueNameField;
    }
}

private string _AddressField;

/// <summary>
/// Access the Address parameter of the template.
/// </summary>
private string Address
{
    get
    {
        return this._AddressField;
    }
}

private string _PhoneNumberField;

/// <summary>
/// Access the PhoneNumber parameter of the template.
/// </summary>
private string PhoneNumber
{
    get
    {
        return this._PhoneNumberField;
    }
}

private string _EmailField;

/// <summary>
/// Access the Email parameter of the template.
/// </summary>
private string Email
{
    get
    {
        return this._EmailField;
    }
}

private string _TypeOfVenueField;

/// <summary>
/// Access the TypeOfVenue parameter of the template.
/// </summary>
private string TypeOfVenue
{
    get
    {
        return this._TypeOfVenueField;
    }
}

private string _ReferralCodeField;

/// <summary>
/// Access the ReferralCode parameter of the template.
/// </summary>
private string ReferralCode
{
    get
    {
        return this._ReferralCodeField;
    }
}


public virtual void Initialize()
{
    if ((this.Errors.HasErrors == false))
    {
bool VenueNameValueAcquired = false;
if (this.Session.ContainsKey("VenueName"))
{
    if ((typeof(string).IsAssignableFrom(this.Session["VenueName"].GetType()) == false))
    {
        this.Error("The type \'System.String\' of the parameter \'VenueName\' did not match the type of t" +
                "he data passed to the template.");
    }
    else
    {
        this._VenueNameField = ((string)(this.Session["VenueName"]));
        VenueNameValueAcquired = true;
    }
}
if ((VenueNameValueAcquired == false))
{
    object data = global::System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("VenueName");
    if ((data != null))
    {
        if ((typeof(string).IsAssignableFrom(data.GetType()) == false))
        {
            this.Error("The type \'System.String\' of the parameter \'VenueName\' did not match the type of t" +
                    "he data passed to the template.");
        }
        else
        {
            this._VenueNameField = ((string)(data));
        }
    }
}
bool AddressValueAcquired = false;
if (this.Session.ContainsKey("Address"))
{
    if ((typeof(string).IsAssignableFrom(this.Session["Address"].GetType()) == false))
    {
        this.Error("The type \'System.String\' of the parameter \'Address\' did not match the type of the" +
                " data passed to the template.");
    }
    else
    {
        this._AddressField = ((string)(this.Session["Address"]));
        AddressValueAcquired = true;
    }
}
if ((AddressValueAcquired == false))
{
    object data = global::System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("Address");
    if ((data != null))
    {
        if ((typeof(string).IsAssignableFrom(data.GetType()) == false))
        {
            this.Error("The type \'System.String\' of the parameter \'Address\' did not match the type of the" +
                    " data passed to the template.");
        }
        else
        {
            this._AddressField = ((string)(data));
        }
    }
}
bool PhoneNumberValueAcquired = false;
if (this.Session.ContainsKey("PhoneNumber"))
{
    if ((typeof(string).IsAssignableFrom(this.Session["PhoneNumber"].GetType()) == false))
    {
        this.Error("The type \'System.String\' of the parameter \'PhoneNumber\' did not match the type of" +
                " the data passed to the template.");
    }
    else
    {
        this._PhoneNumberField = ((string)(this.Session["PhoneNumber"]));
        PhoneNumberValueAcquired = true;
    }
}
if ((PhoneNumberValueAcquired == false))
{
    object data = global::System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("PhoneNumber");
    if ((data != null))
    {
        if ((typeof(string).IsAssignableFrom(data.GetType()) == false))
        {
            this.Error("The type \'System.String\' of the parameter \'PhoneNumber\' did not match the type of" +
                    " the data passed to the template.");
        }
        else
        {
            this._PhoneNumberField = ((string)(data));
        }
    }
}
bool EmailValueAcquired = false;
if (this.Session.ContainsKey("Email"))
{
    if ((typeof(string).IsAssignableFrom(this.Session["Email"].GetType()) == false))
    {
        this.Error("The type \'System.String\' of the parameter \'Email\' did not match the type of the d" +
                "ata passed to the template.");
    }
    else
    {
        this._EmailField = ((string)(this.Session["Email"]));
        EmailValueAcquired = true;
    }
}
if ((EmailValueAcquired == false))
{
    object data = global::System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("Email");
    if ((data != null))
    {
        if ((typeof(string).IsAssignableFrom(data.GetType()) == false))
        {
            this.Error("The type \'System.String\' of the parameter \'Email\' did not match the type of the d" +
                    "ata passed to the template.");
        }
        else
        {
            this._EmailField = ((string)(data));
        }
    }
}
bool TypeOfVenueValueAcquired = false;
if (this.Session.ContainsKey("TypeOfVenue"))
{
    if ((typeof(string).IsAssignableFrom(this.Session["TypeOfVenue"].GetType()) == false))
    {
        this.Error("The type \'System.String\' of the parameter \'TypeOfVenue\' did not match the type of" +
                " the data passed to the template.");
    }
    else
    {
        this._TypeOfVenueField = ((string)(this.Session["TypeOfVenue"]));
        TypeOfVenueValueAcquired = true;
    }
}
if ((TypeOfVenueValueAcquired == false))
{
    object data = global::System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("TypeOfVenue");
    if ((data != null))
    {
        if ((typeof(string).IsAssignableFrom(data.GetType()) == false))
        {
            this.Error("The type \'System.String\' of the parameter \'TypeOfVenue\' did not match the type of" +
                    " the data passed to the template.");
        }
        else
        {
            this._TypeOfVenueField = ((string)(data));
        }
    }
}
bool ReferralCodeValueAcquired = false;
if (this.Session.ContainsKey("ReferralCode"))
{
    if ((typeof(string).IsAssignableFrom(this.Session["ReferralCode"].GetType()) == false))
    {
        this.Error("The type \'System.String\' of the parameter \'ReferralCode\' did not match the type o" +
                "f the data passed to the template.");
    }
    else
    {
        this._ReferralCodeField = ((string)(this.Session["ReferralCode"]));
        ReferralCodeValueAcquired = true;
    }
}
if ((ReferralCodeValueAcquired == false))
{
    object data = global::System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("ReferralCode");
    if ((data != null))
    {
        if ((typeof(string).IsAssignableFrom(data.GetType()) == false))
        {
            this.Error("The type \'System.String\' of the parameter \'ReferralCode\' did not match the type o" +
                    "f the data passed to the template.");
        }
        else
        {
            this._ReferralCodeField = ((string)(data));
        }
    }
}


    }
}


        
        #line default
        #line hidden
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public class SubscriptionCancelEmailBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
