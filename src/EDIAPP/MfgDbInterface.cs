using MfgConnection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDIAPP
{
    public class EdiDbInterface
    {
        public readonly string connectionStringToMfgDb;
        public readonly string applicationName;
        public readonly string applicationVersion;
        public readonly string userName;
        public readonly string machineName;
        public enum Settings
        {
            id,
            userName,
            workstation,
            engine,
            soCreation,
            soCreationStatus,
            executionType,
            template,
            clients,
            filetype,
            extras,
            writeScheduleLineToNote,
            jbDbName,
            jbDbServer,
            active,
            title,
            useShippedLines
        };


        public EdiDbInterface()
        {
            applicationName = "EDI";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            applicationVersion = version.ToString();
            userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            machineName = System.Environment.MachineName;

            var ediDbName = "MFG_EDI";
            var conn = new jbConnection();
            var dbServerLocation = conn.GetJbSettingsServer();

            connectionStringToMfgDb = "Data Source=" + dbServerLocation
            + ";Initial Catalog=" + ediDbName
            + ";User ID=jobboss;Password=Bali;User Instance=false;"
            + "MultipleActiveResultSets=true";

            try
            {
                var connectionToMfgDb = new System.Data.SqlClient.SqlConnection(
                  connectionStringToMfgDb);
                connectionToMfgDb.Open();
                connectionToMfgDb.Close();
            }
            catch (Exception e)
            {
                throw new DatabaseNotFoundException(e.Message);
            }
        }

        public object[][] AcquireAllConfigurationsForUserAndStation()
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(
              connectionStringToMfgDb))
            {
                connection.Open();
                var query = "SELECT id, userName, workstation, engine, soCreation, "
                  + "soCreationStatus, executionType, template, clients, filetype, "
                  + "extras, writeScheduleLineToNote, jbDbName, jbDbServer, active, "
                  + "title, useShippedLines "
                  + "FROM Edi_Config WHERE userName = @userName AND workstation = @machineName";

                var command = new System.Data.SqlClient.SqlCommand(query, connection);
                command.Parameters.AddWithValue("@userName", userName);
                command.Parameters.AddWithValue("@machineName", machineName);
                var reader = command.ExecuteReader();

                try
                {
                    if (reader.HasRows)
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            object[][] results = new object[dt.Rows.Count][];
                            foreach (DataRow dr in dt.Rows)
                            {
                                results[dt.Rows.IndexOf(dr)] = dr.ItemArray;
                            }

                            return results;
                        }
                    }
                    else
                    {
                        var message = "Il n'y a pas de configuration pour l'usager("
                          + userName + ") de la station(" + machineName + ")";
                        throw new NoConfigsFoundException(message);
                    }
                }
                finally
                {
                }
            }

            return null;
        }

        public void DeleteSettingWithId(string id)
        {
            var query = "DELETE FROM Edi_Config WHERE id = @id";

            using (var connection = new System.Data.SqlClient.SqlConnection(
                connectionStringToMfgDb))
            {
                connection.Open();

                var command = new System.Data.SqlClient.SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }

        }

        public void InsertRefusalForDateReason(string refusal)
        {

        }

        public void InsertRefusalForQuantityReason(string refusal)
        {

        }

        private void MakeOtherSettingsInactiveForUserAndStation(in string user,
            in string station, in string id = "")
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(
              connectionStringToMfgDb))
            {
                connection.Open();
                var query = "UPDATE Edi_Config SET active = 0 WHERE userName = @username "
                + "AND workstation = @station";
                if (!String.IsNullOrEmpty(id))
                {
                    query += " AND NOT id = @id";
                }

                var command = new System.Data.SqlClient.SqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", user);
                command.Parameters.AddWithValue("@station", station);
                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();
            }
        }

        public object[] ReadBasicSettingsFromEdiMfgTables()
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(
              connectionStringToMfgDb))
            {
                connection.Open();
                var query = "SELECT id, userName, workstation, engine, soCreation, "
                  + "soCreationStatus, executionType, template, clients, filetype, "
                  + "extras, writeScheduleLineToNote, jbDbName, jbDbServer, active, "
                  + "title, useShippedLines "
                  + "FROM Edi_Config WHERE userName = @userName AND workstation = @machineName";

                var command = new System.Data.SqlClient.SqlCommand(query, connection);
                command.Parameters.AddWithValue("@userName", userName);
                command.Parameters.AddWithValue("@machineName", machineName);
                var reader = command.ExecuteReader();

                try
                {
                    if (reader.HasRows)
                    {
                        var dt = new System.Data.DataTable();
                        dt.Load(reader);

                        if (dt.Rows.Count == 1)
                        {
                            return dt.Rows[0].ItemArray;
                        }

                        if (dt.Rows.Count > 1)
                        {
                            DataRow[] foundRows = dt.Select("active = True");
                            if (foundRows.Length > 0)
                            {
                                return foundRows[0].ItemArray;
                            }

                            return dt.Rows[0].ItemArray;
                        }
                    }
                    else
                    {
                        var message = "Il n'y a pas de configuration pour l'usager("
                          + userName + ") de la station(" + machineName + ")";
                        throw new NoConfigsFoundException(message);
                    }
                }
                finally
                {
                }
            }

            return null;
        }

        public object[] ReadRefusalsForDateReasons()
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionStringToMfgDb))
            {
                connection.Open();
                var query = "SELECT reason FROM edi_date_refusals";

                var command = new System.Data.SqlClient.SqlCommand(query, connection);
                var reader = command.ExecuteReader();

                try
                {
                    if (reader.HasRows)
                    {
                        var dt = new System.Data.DataTable();
                        dt.Load(reader);

                        var refusals = new object[dt.Rows.Count];

                        foreach (DataRow row in dt.Rows)
                        {
                            refusals[dt.Rows.IndexOf(row)] = row[0];
                        }
                        return refusals;
                    }
                    else
                    {
                        var message = "Il n'y a pas de raisons de refus de date dans la "
                          + "base de données. SVP de remédier à la situation.";
                        throw new NoConfigsFoundException(message);
                    }
                }
                finally
                {
                }
            }

            return null;
        }

        public object[] ReadRefusalsForQuantityReasons()
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionStringToMfgDb))
            {
                connection.Open();
                var query = "SELECT reason FROM edi_quantity_refusals";

                var command = new System.Data.SqlClient.SqlCommand(query, connection);
                var reader = command.ExecuteReader();

                try
                {
                    if (reader.HasRows)
                    {
                        var dt = new System.Data.DataTable();
                        dt.Load(reader);

                        var refusals = new object[dt.Rows.Count];

                        foreach (DataRow row in dt.Rows)
                        {
                            refusals[dt.Rows.IndexOf(row)] = row[0];
                        }
                        return refusals;
                    }
                    else
                    {
                        var message = "Il n'y a pas de raisons de refus de date dans la "
                          + "base de données. SVP de remédier à la situation.";
                        throw new NoConfigsFoundException(message);
                    }
                }
                finally
                {
                }
            }

            return null;
        }

        public string ReadSetting(string settingName)
        {
            return ""; //TODO (@mond): Fake implementation
        }

        public void RemoveRefusalForDateReason(string refusal)
        {

        }
        public void RemoveRefusalForDateReasonAtIndex(int index)
        {

        }
        public void RemoveRefusalForQuantityReason(string refusal)
        {

        }
        public void RemoveRefusalForQuantityReasonAtIndex(int index)
        {

        }

        public void SetSetting(string setting, string value)
        {

        }

        public void UpdateBasicSettings(in List<string> settingsToWrite)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(
              connectionStringToMfgDb))
            {
                connection.Open();

                var query = "UPDATE Edi_Config SET engine = @engine, "
                  + "soCreation = @soCreation, soCreationStatus = @soCreationStatus, "
                  + "executionType = @execution, template = @template, "
                  + "clients = @clients, filetype = @filetypes, extras = @extras, "
                  + "writeScheduleLineToNote = @writeScheduleLineToNote, "
                  + "jbDbName = @dbName, jbDbServer = @dbServer, active = @active, "
                  + "title = @title , useShippedLines = @useShippedLines WHERE id = @id";

                var command = new System.Data.SqlClient.SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", settingsToWrite[(int)Settings.id]);
                command.Parameters.AddWithValue("@engine", settingsToWrite[(int)Settings.engine]);
                command.Parameters.AddWithValue("@soCreation",
                  settingsToWrite[(int)Settings.soCreation]);
                command.Parameters.AddWithValue("@soCreationStatus",
                  settingsToWrite[(int)Settings.soCreationStatus]);
                command.Parameters.AddWithValue("@execution",
                  settingsToWrite[(int)Settings.executionType]);
                command.Parameters.AddWithValue("@template",
                  settingsToWrite[(int)Settings.template]);
                command.Parameters.AddWithValue("@clients",
                  settingsToWrite[(int)Settings.clients]);
                command.Parameters.AddWithValue("@fileTypes",
                  settingsToWrite[(int)Settings.filetype]);
                command.Parameters.AddWithValue("@extras", settingsToWrite[(int)Settings.extras]);
                command.Parameters.AddWithValue("@writeScheduleLineToNote",
                  settingsToWrite[(int)Settings.writeScheduleLineToNote]);
                command.Parameters.AddWithValue("@dbName",
                  settingsToWrite[(int)Settings.jbDbName]);
                command.Parameters.AddWithValue("@dbServer",
                  settingsToWrite[(int)Settings.jbDbServer]);
                command.Parameters.AddWithValue("@active", settingsToWrite[(int)Settings.active]);
                command.Parameters.AddWithValue("@title", settingsToWrite[(int)Settings.title]);
                command.Parameters.AddWithValue("@useShippedLines",
                    settingsToWrite[(int)Settings.useShippedLines]);

                command.ExecuteNonQuery();

                if (settingsToWrite[(int)Settings.active] == "1")
                {
                    MakeOtherSettingsInactiveForUserAndStation(settingsToWrite[(int)Settings.userName],
                        settingsToWrite[(int)Settings.workstation], settingsToWrite[(int)Settings.id]);
                }
            }
        }

        public void WriteBasicSettings(in List<string> settingsToWrite)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(
              connectionStringToMfgDb))
            {
                connection.Open();
                var query = "INSERT INTO Edi_Config(userName, workstation, engine, "
                  + "soCreation, soCreationStatus, executionType, template, clients, "
                  + "filetype, extras, writeScheduleLineToNote, jbDbName, jbDbServer, "
                  + "active, title, useShippedLines) "
                  + "VALUES(@user, @station, @engine, @soCreation, @soCreationStatus, "
                  + "@execution, @template, @clients, @fileTypes, @extras, "
                  + "@writeScheduleLineToNote, @dbName, @dbServer, @active, @title, @useShippedLines)";

                var command = new System.Data.SqlClient.SqlCommand(query, connection);
                command.Parameters.AddWithValue("@user", settingsToWrite[(int)Settings.userName]);
                command.Parameters.AddWithValue("@station",
                  settingsToWrite[(int)Settings.workstation]);
                command.Parameters.AddWithValue("@engine", settingsToWrite[(int)Settings.engine]);
                command.Parameters.AddWithValue("@soCreation",
                  settingsToWrite[(int)Settings.soCreation]);
                command.Parameters.AddWithValue("@soCreationStatus",
                  settingsToWrite[(int)Settings.soCreationStatus]);
                command.Parameters.AddWithValue("@execution",
                  settingsToWrite[(int)Settings.executionType]);
                command.Parameters.AddWithValue("@template",
                  settingsToWrite[(int)Settings.template]);
                command.Parameters.AddWithValue("@clients",
                  settingsToWrite[(int)Settings.clients]);
                command.Parameters.AddWithValue("@fileTypes",
                  settingsToWrite[(int)Settings.filetype]);
                command.Parameters.AddWithValue("@extras", settingsToWrite[(int)Settings.extras]);
                command.Parameters.AddWithValue("@writeScheduleLineToNote",
                  settingsToWrite[(int)Settings.writeScheduleLineToNote]);
                command.Parameters.AddWithValue("@dbName",
                  settingsToWrite[(int)Settings.jbDbName]);
                command.Parameters.AddWithValue("@dbServer",
                  settingsToWrite[(int)Settings.jbDbServer]);
                command.Parameters.AddWithValue("@active", settingsToWrite[(int)Settings.active]);
                command.Parameters.AddWithValue("@title", settingsToWrite[(int)Settings.title]);

                if (settingsToWrite[(int)Settings.active] == "1")
                {
                    MakeOtherSettingsInactiveForUserAndStation(settingsToWrite[(int)Settings.userName],
                        settingsToWrite[(int)Settings.workstation]);
                }

                command.Parameters.AddWithValue("@useShippedLines",
                  settingsToWrite[(int)Settings.useShippedLines]);

                command.ExecuteNonQuery();
            }
        }

        public void WriteRefusalsForDateReasons(ref List<string> refusals)
        {
            //TODO (@mond): Do we need to implement this?
        }

        public void WriteRefusalsForQuantityReasons(ref List<string> refusals)
        {
            //TODO (@mond): Do we need to implement this?
        }
    }
}