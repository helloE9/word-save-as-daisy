using Daisy.SaveAsDAISY.Conversion.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Daisy.SaveAsDAISY.Conversion
{
    public class BoolDataType : ParameterDataType
    {
        private bool m_Val;
        private readonly string m_strTrue_Val  = "true";    // string value being used by true flag in script
        private readonly string m_strFalse_Val = "false";   // string value being used by False flag in script

        public BoolDataType(ScriptParameter p, XmlNode DataTypeNode) : base(p)
        {
            XmlNode FirstChild = DataTypeNode.FirstChild;
            m_Val = Convert.ToBoolean(p.ParameterValue);
            if (FirstChild.Attributes.Count>0)
            {
                m_strTrue_Val  = FirstChild.Attributes.GetNamedItem("true").Value;
                m_strFalse_Val = FirstChild.Attributes.GetNamedItem("false").Value;
            }
        }

        public BoolDataType(bool defaultValue = false) : base() {
            m_Val = defaultValue;
        }

        public bool Value
        {
            get { return m_Val; }
            set
            {
                m_Val = value;
                if (value == true)
                    m_Parameter.ParameterValue = m_strTrue_Val;
                else
                    m_Parameter.ParameterValue = m_strFalse_Val;
            }
        }

    }

}
