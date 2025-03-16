namespace IfcToolbox.Tools.Configurations
{
    public static class ConfigFactory
    {
        /// <summary>
        ///  keepLabel - Conserve old entity label.
        ///  DeleteOld - Reorder all entity label and remove unused entity. (Performance related)
        ///  LogDetail - Log process details in console mode (Performance related)
        /// </summary>
        public static IConfigBase CreateConfigBase(bool keepLabel = false, bool deleteOld = true, bool logDetail = false)
        {
            var config = new ConfigBase();
            config.KeepLabel = keepLabel;
            config.DeleteOld = deleteOld;
            config.LogDetail = logDetail;
            return config;
        }

        public static IConfigSplit CreateConfigSplit()
        {
            return new ConfigSplit();
        }
    }
}
