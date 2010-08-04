﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.QueryLanguage.Result;
using sones.GraphDB.Settings;
using sones.GraphDB.QueryLanguage.Enums;
using sones.Lib.ErrorHandling;
using sones.GraphDB.Errors;
using sones.GraphDB.TypeManagement.PandoraTypes;
using sones.GraphDB.TypeManagement;

namespace sones.GraphDB.Managers.Structures.Setting
{

    public class SettingDBDefinition : ASettingDefinition
    {

        #region override ASettingDefinition.*

        public override Exceptional<List<SelectionResultSet>> ExtractData(Dictionary<string, string> mySetting, DBContext _DBContext)
        {
            ADBSettingsBase Setting = null;
            List<SelectionResultSet> result = new List<SelectionResultSet>();
            List<DBObjectReadout> SettingList = new List<DBObjectReadout>();
            Dictionary<string, ADBSettingsBase> _Settings = new Dictionary<string, ADBSettingsBase>(); ;

            foreach (var pSetting in mySetting)
            {

                Setting = _DBContext.DBSettingsManager.GetSetting(pSetting.Key.ToUpper(), _DBContext, TypesSettingScope.DB).Value;
                if (Setting != null && !_Settings.ContainsKey(Setting.Name))
                {
                    _Settings.Add(Setting.Name, (ADBSettingsBase)Setting.Clone());
                }


                var SettingPair = MakeOutputForAttribs(Setting);
                SettingList.Add(new DBObjectReadout(SettingPair));
            }

            result.Add(new SelectionResultSet(SettingList));

            return new Exceptional<List<SelectionResultSet>>(result);
        }

        public override Exceptional<List<SelectionResultSet>> SetData(Dictionary<string, string> mySettingValues, DBContext _DBContext)
        {

            foreach (var pSetting in mySettingValues)
            {
                _DBContext.DBSettingsManager.SetSetting(pSetting.Key, GetValueForSetting(_DBContext.DBSettingsManager.AllSettingsByName[pSetting.Key], pSetting.Value), _DBContext, TypesSettingScope.DB);
            }

            List<DBObjectReadout> resultingReadouts = new List<DBObjectReadout>();
            resultingReadouts.Add(CreateNewSettingReadoutOnSet(TypesSettingScope.DB, mySettingValues));
            return new Exceptional<List<SelectionResultSet>>(new List<SelectionResultSet>() { new SelectionResultSet(resultingReadouts) });

        }

        public override Exceptional<List<SelectionResultSet>> RemoveData(Dictionary<String, String> mySettings, DBContext _DBContext)
        {
            foreach (var Setting in mySettings)
            {

                var removeResult = _DBContext.DBSettingsManager.RemoveSetting(_DBContext, Setting.Key.ToUpper(), TypesSettingScope.DB);
                if (removeResult.Failed)
                {
                    return new Exceptional<List<SelectionResultSet>>(removeResult);
                }
            }

            return new Exceptional<List<SelectionResultSet>>();

        }

        #endregion

    }

}
