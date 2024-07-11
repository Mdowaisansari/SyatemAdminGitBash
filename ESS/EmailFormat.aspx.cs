﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using SystemAdmin.App_Code;

namespace SystemAdmin.ESS
{
    public partial class EmailFormat : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserAutoId"] == null)
            {
                Response.Redirect("~/Default.aspx");
            }
            if (!Page.IsPostBack)
            {
                FillGroup(); 
                FillListView(); 
            }
        } 
        private void BindCheckBoxList()
        {
            chkGroupCompany.DataSource = GetGroup();
            chkGroupCompany.DataBind();
        }
        protected void chkGroupCompany_DataBinding(object sender, EventArgs e)
        {
            ((CheckBoxList)sender).DataSource = GetGroup();
        }
        public DataTable GetGroup()
        {
            DropdownPL PL = new DropdownPL();
            PL.OpCode = 3;
            PL.AutoId = 1;
            DropdownDL.returnTable(PL);
            return PL.dt;
        }
        void ddlGetGroup()
        {
            DropdownPL PL = new DropdownPL();
            PL.OpCode = 3;
            PL.AutoId = 1;
            DropdownDL.returnTable(PL);
            ddlUpdateGroupCompany.DataSource = PL.dt;
            ddlUpdateGroupCompany.DataValueField = "AutoId";
            ddlUpdateGroupCompany.DataTextField = "Name";
            ddlUpdateGroupCompany.DataBind();
        }
        void FillListView()
        {
            //-----------------
            ServiceMasterPL PL = new ServiceMasterPL();
            PL.OpCode = 21;
            PL.Type = ddlTypeSearch.SelectedValue;
            PL.GroupId = ddlGroupFilter.SelectedValue;
            ServiceMasterDL.returnTable(PL);
            DataTable dt = PL.dt;
            //--------------------------------
            LV.DataSource = dt;
            LV.DataBind();
        }
        void FillGroup()
        {
            DropdownPL PL = new DropdownPL();
            PL.OpCode = 23;
            DropdownDL.returnTable(PL);
            ddlGroupFilter.DataSource = PL.dt;
            ddlGroupFilter.DataValueField = "GroupId";
            ddlGroupFilter.DataTextField = "Name";
            ddlGroupFilter.DataBind();
        }
        void ClearField()
        {
            txtFunctionName.Text = "";
            txtname.Text = "";
            txtDescription.Text = "";
            ckObjectives.InnerText = "";
            txtFunctionName.Attributes.Add("oldname", "");
            ddlType.SelectedIndex = -1;  
            chkactive.Checked = true; 
            hidID.Value = "";
        } 
        protected void lnkBtnAddNew_Click(object sender, EventArgs e)
        {
            ClearField();
            divView.Visible = false;
            divAddEdit.Visible = true;
            ViewState["Mode"] = "Add";
            divUpdateGroup.Visible = false;
            divAddGroup.Visible = true;
            BindCheckBoxList(); 
            ddlUpdateGroupCompany.Items.Clear();
        }
        protected void ddlUpdateGroupCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            setForEdit(Convert.ToInt32(ddlUpdateGroupCompany.SelectedValue), Convert.ToInt32(hidID.Value));
        }
        protected void lnkBtnEdit_Click(object sender, EventArgs e)
        {
            ddlGetGroup();
            foreach (ListViewItem item in LV.Items)
            {
                CheckBox chkSelect = (CheckBox)item.FindControl("chkSelect");
                if (chkSelect != null)
                {
                    if (chkSelect.Checked)
                    {  
                        int Autoid = Convert.ToInt32(chkSelect.Attributes["Autoid"]);
                        hidGroupID.Value = chkSelect.Attributes["GroupId"];
                        hidID.Value = Autoid.ToString(); 
                        setForEdit(Convert.ToInt32(hidGroupID.Value), Autoid);
                    }
                }
            }
            divView.Visible = false;
            divAddEdit.Visible = true;
            ViewState["Mode"] = "Edit";
            divUpdateGroup.Visible = true;
            divAddGroup.Visible = false;
        }
        void setForEdit(int groupId, int id)
        {
            ServiceMasterPL PL = new ServiceMasterPL();
            PL.OpCode = 24;
            PL.GroupId = groupId;
            PL.AutoId = id;
            ServiceMasterDL.returnTable(PL);
            DataTable dt = PL.dt;
            //--------------------------------
            if (dt.Rows.Count > 0)
            {
                txtFunctionName.Text = dt.Rows[0]["Name"].ToString();
                txtname.Text = dt.Rows[0]["EmailName"].ToString();
                txtDescription.Text = dt.Rows[0]["Description"].ToString();
                ddlUpdateGroupCompany.SelectedIndex = ddlUpdateGroupCompany.Items.IndexOf(ddlUpdateGroupCompany.Items.FindByValue(dt.Rows[0]["GroupId"].ToString())); 
                ddlType.SelectedIndex = ddlType.Items.IndexOf(ddlType.Items.FindByValue(dt.Rows[0]["Type"].ToString()));
                ckObjectives.InnerText = dt.Rows[0]["Body"].ToString();
                chkactive.Checked = bool.Parse(dt.Rows[0]["IsActive"].ToString()); 
                ViewState["Mode"] = "Edit";
                divView.Visible = false;
                divAddEdit.Visible = true;
            }
            else
            {
                ClearField();
            }
        }  
        protected void btncancel_Click(object sender, EventArgs e)
        {
            divView.Visible = true;
            divAddEdit.Visible = false;
        }
        string GetParentServiceXml()
        {
            string xml = "<tbl>";
            xml += "<tr>";
            xml += "<Name><![CDATA[" + txtFunctionName.Text.Trim() + "]]></Name>";
            xml += "<EmailName><![CDATA[" + txtname.Text.Trim() + "]]></EmailName>";
            xml += "<Description><![CDATA[" + txtDescription.Text.Trim() + "]]></Description>";
            xml += "<Type><![CDATA[" + ddlType.SelectedValue + "]]></Type>";
            xml += "<IsActive><![CDATA[" + (chkactive.Checked) + "]]></IsActive>";
            xml += "</tr>";
            xml += "</tbl>";
            return xml;
        }   
        private bool IsAnyItemChecked()
        {
            foreach (ListItem item in chkGroupCompany.Items)
            {
                if (item.Selected)
                {
                    return true;
                }
            }
            return false;
        } 
        protected void btnsave_Click(object sender, EventArgs e)
        {
            if (IsAnyItemChecked() || ddlUpdateGroupCompany.SelectedValue != "")
            {
                ServiceMasterPL PL = new ServiceMasterPL();
                if (ViewState["Mode"].ToString() == "Add")
                {
                    PL.OpCode = 22;
                    PL.XML = GetParentServiceXml();
                    //PL.XML1 = saveGroup();
                    PL.CreatedBy = Session["UserAutoId"].ToString();
                    ServiceMasterDL.returnTable(PL);
                    if (!PL.isException)
                    {
                        insertEmailConentXml(PL.dt.Rows[0]["MainEmailId"].ToString());
                        divView.Visible = true;
                        divAddEdit.Visible = false;
                        ClearField();
                        FillListView();
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "flagSave", "ShowDone('Record save successfully');", true);
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "flagError", "ShowError('" + PL.exceptionMessage + "');", true);
                    }
                }
                else if (ViewState["Mode"].ToString() == "Edit")
                {
                    PL.OpCode = 25;
                    PL.XML = GetParentServiceXml(); 
                    PL.AutoId = Convert.ToInt32(hidID.Value);
                    PL.CreatedBy = Session["UserAutoId"].ToString();
                    ServiceMasterDL.returnTable(PL);
                    if (!PL.isException)
                    {
                        updateEmailConentXml(hidID.Value);
                        divView.Visible = true;
                        divAddEdit.Visible = false;
                        ClearField();
                        FillListView();
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "flagSave", "ShowDone('Record save successfully');", true);
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "flagError", "ShowError('" + PL.exceptionMessage + "');", true);
                    }
                }
                ClearField();
                FillGroup();
                FillListView();
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "flag", "ShowError('Select atleast one Company');", true);
            }
        }  

        [System.Web.Services.WebMethod]
        public static string CheckName(string text, string oldname)
        {
            ServiceMasterPL PL = new ServiceMasterPL();
            PL.OpCode = 14;
            PL.Type = text;
            PL.OldName = oldname;
            ServiceMasterDL.returnTable(PL); ;
            return PL.dt.Rows[0]["count"].ToString();
        }
        [System.Web.Services.WebMethod]
        public static string QuestionsData(string DocumentId)
        {
            string jsondata = "";
            ServiceMasterPL PL = new ServiceMasterPL();
            PL.OpCode = 15;
            PL.AutoId = Convert.ToInt32(DocumentId);
            ServiceMasterDL.returnTable(PL);
            if (PL.dt.Rows.Count > 0)
            {
                jsondata = new clsGeneral().JSONfromDT(PL.dt);
            }
            return jsondata;
        }

        protected void ddlGroupFilter_SelectedIndexChanged(object sender, EventArgs e)
        { 
            FillListView();
        }  
        void insertEmailConentXml(string mainEmailId)
        {
            ServiceMasterPL PL = new ServiceMasterPL();
            foreach (ListItem gr in chkGroupCompany.Items)
            {
                if (gr.Selected)
                {
                    string xml = "<tbl>";
                    xml += setEmailContentXML(mainEmailId, gr.Value);
                    xml += "</tbl>"; 
                    PL.OpCode = 23;
                    PL.XML1 = xml;
                    ServiceMasterDL.returnTable(PL);
                    if (!PL.isException)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "flag", "ShowDone('Email Format Add successfully');", true);
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "flag", "ShowError('Select atleast one Company');", true);
                    }
                }
            }
        }
        void updateEmailConentXml(string mainEmailId)
        {
            ServiceMasterPL PL = new ServiceMasterPL();
            string xml = "<tr>";
            xml += "<mainEmailId><![CDATA[" + mainEmailId + "]]></mainEmailId>"; 
            xml += "<Content><![CDATA[" + ckObjectives.InnerText + "]]></Content>";
            xml += "<Description><![CDATA[" + txtDescription.Text + "]]></Description>";
            xml += "</tr>";

            PL.CreatedBy = Session["UserAutoId"].ToString();
            PL.AutoId = mainEmailId;
            PL.GroupId = ddlUpdateGroupCompany.SelectedValue;
            PL.OpCode = 26;
            PL.XML1 = xml;
            ServiceMasterDL.returnTable(PL);
            if (!PL.isException)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "flag", "ShowDone('Email Format Add successfully');", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "flag", "ShowError('Select atleast one Company');", true);
            }
        }
        string setEmailContentXML(string mainid, string GroupId)
        {
            string xml = "<tr>";
            xml += "<mainEmailId><![CDATA[" + mainid + "]]></mainEmailId>"; 
            xml += "<GroupId><![CDATA[" + GroupId + "]]></GroupId>";
            xml += "<Content><![CDATA[" + ckObjectives.InnerText + "]]></Content>";
            xml += "</tr>";
            return xml;
        }

        protected void ddlTypeSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillListView();
        }
    }
}