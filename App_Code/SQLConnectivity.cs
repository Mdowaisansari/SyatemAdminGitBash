﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
public class SQLConnectivity
{
    public SqlConnection SqlCon = new SqlConnection(WebConfigurationManager.ConnectionStrings["constr"].ToString());

    public SQLConnectivity()
    {
        SqlCon = new SqlConnection(WebConfigurationManager.ConnectionStrings["constr"].ToString());
    }
    public ConnectionState State { get; set; }
}