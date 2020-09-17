using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetDbHelp
{
    public class CmdInfo
    {
        public string CommandText;
        public SqlParameter[] Parameters;
        public int CmdType;

        public CmdInfo() { }

        public CmdInfo(string comText,int cmdType) {
            this.CommandText = comText;
            this.CmdType = cmdType;
        }
        public CmdInfo(string comText,SqlParameter[] paras, int cmdType)
        {
            this.CommandText = comText;
            this.Parameters = paras;
            this.CmdType = cmdType;
        }

    }
}
