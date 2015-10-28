﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace JointMilitarySymbologyLibrary
{
    public class SchemaETL
    {
        private const string _headers = "field_name,field_type,field_length,field_alias,nullability,field_domain,field_default,field_notes";
        private const string _schemaHeaders = "schema_class,schema_name,schema_geometry,schema_alias,schema_label,schema_thumb,schema_tags,schema_summary,schema_description,schema_credits,schema_use,schema_east,schema_west,schema_north,schema_south,schema_maxscale,schema_minscale,schema_spatial_reference";
        private const string _filePrefix = "Fields_";
        private const string _schemaListFile = "Schemas.csv";
        private const string _testedFilePrefix = "tested_";
        private const string _testFolder = "..\\..\\test";
        private const string _outputFolder = "\\comparison_test_results";
        private const string _testField = "field_comparison_result";
        private const string _nullable = "NULLABLE";
        private const string _nonnullable = "NON_NULLABLE";

        private char[] _comma = { ',' };
        private char[] _semicolon = { ';', ' ' };

        private List<string> _testResults = new List<string>();
        
        private JMSMLConfigETLConfig _etlConfig;
        

        public SchemaETL(JMSMLConfigETLConfig config)
        {
            _etlConfig = config;
        }

        private void _writeHeaders(StreamWriter w)
        {
            w.WriteLine(_headers);
            w.Flush();
        }

        private void _writeField(StreamWriter w, FieldType field)
        {
            string line = string.Format("{0}", field.Value + "," +
                                               field.Type + "," +
                                               field.Length + "," +
                                               field.Alias + "," +
                                               (field.IsNullable ? _nullable : _nonnullable) + "," +
                                               field.Domain + "," +
                                               field.Default + "," +
                                               field.Notes);
            w.WriteLine(line);
            w.Flush();
        }

        private JMSMLConfigETLConfigSchemaType _findSchemaType(string id)
        {
            // Find a schema type given its ID.

            JMSMLConfigETLConfigSchemaType result = null;

            if (_etlConfig != null)
            {
                foreach (JMSMLConfigETLConfigSchemaType schemaType in _etlConfig.SchemaTypes)
                {
                    if (id == schemaType.ID)
                    {
                        result = schemaType;
                        break;
                    }
                }
            }

            return result;
        }

        private FieldType[] _buildFieldArray(JMSMLConfigETLConfigSchemaContainerSchemasSchema schema)
        {
            // Build an order-sorted array of fields for the given schema.

            FieldType[] fields = {};

            string[] schemaTypes = schema.SchemaTypeIDs.Split(' ');

            // For each schema type in the schema, fined that type and fetch its fields.
            // Then concatenate the fields.

            foreach (string id in schemaTypes)
            {
                JMSMLConfigETLConfigSchemaType schemaType = _findSchemaType(id);

                if(schemaType != null)
                    fields = fields.Concat(schemaType.Fields).ToArray();
            }

            // Now concat the fields that are unique to this schema.

            fields = fields.Concat(schema.Fields).ToArray();

            // Now sort the array of fields based on their Order property.

            fields = fields.OrderBy(x => int.Parse(x.Order)).ToArray();

            return fields;
        }

        private void _exportFieldSchema(string path, JMSMLConfigETLConfigSchemaContainerSchemasSchema schema)
        {
            // Export a single schema to the given path.

            using (var w = new StreamWriter(path + "\\" + _filePrefix + schema.Label + ".csv"))
            {
                _writeHeaders(w);

                FieldType[] fields = _buildFieldArray(schema);

                foreach (FieldType field in fields)
                {
                    _writeField(w, field);
                }
            }
        }

        private bool _exportSchemaContainer(string path, JMSMLConfigETLConfigSchemaContainer container, bool isFirst)
        {
            // Export the information needed to create a container to hold schemas

            bool firstTime = isFirst;

            using (var w = new StreamWriter(path + "\\" + _schemaListFile, !firstTime))
            {
                if (firstTime)
                {
                    w.WriteLine(_schemaHeaders);
                    w.Flush();

                    firstTime = false;
                }

                string line = string.Format("{0}", "SchemaContainer," +
                                                    container.Label + "," +
                                                    "Mixed" + "," +
                                                    '"' + container.LabelAlias + '"' + "," +
                                                    '"' + container.Metadata.Label + '"' + "," +
                                                    container.Metadata.Thumbnail + "," +
                                                    container.Metadata.Tags + "," +
                                                    '"' + container.Metadata.Summary + '"' + "," +
                                                    '"' + container.Metadata.Description + '"' + "," +
                                                    (container.Metadata.Credits != null ? '"' + container.Metadata.Credits + '"' : "") + "," +
                                                    '"' + container.Metadata.Use + '"' + "," +
                                                    (container.Metadata.Extent != null ? Convert.ToString(container.Metadata.Extent.East) : "") + "," +
                                                    (container.Metadata.Extent != null ? Convert.ToString(container.Metadata.Extent.West) : "") + "," +
                                                    (container.Metadata.Extent != null ? Convert.ToString(container.Metadata.Extent.North) : "") + "," +
                                                    (container.Metadata.Extent != null ? Convert.ToString(container.Metadata.Extent.South) : "") + "," +
                                                    container.Metadata.MaximumScale + "," +
                                                    container.Metadata.MinimumScale + "," +
                                                    "");
                w.WriteLine(line);
                w.Flush();
            }

            return firstTime;
        }

        private bool _exportSchemaSet(string path, JMSMLConfigETLConfigSchemaContainerSchemas schemas, bool isFirst)
        {
            // Export the information needed to create a set of related schemas within the schema container

            bool firstTime = isFirst;

            using (var w = new StreamWriter(path + "\\" + _schemaListFile, !firstTime))
            {
                if (firstTime)
                {
                    w.WriteLine(_schemaHeaders);
                    w.Flush();

                    firstTime = false;
                }

                string line = string.Format("{0}", "SchemaSet," +
                                                    schemas.Label + "," +
                                                    "Mixed" + "," +
                                                    '"' + schemas.LabelAlias + '"' + "," +
                                                    '"' + schemas.Metadata.Label + '"' + "," +
                                                    schemas.Metadata.Thumbnail + "," +
                                                    schemas.Metadata.Tags + "," +
                                                    '"' + schemas.Metadata.Summary + '"' + "," +
                                                    '"' + schemas.Metadata.Description + '"' + "," +
                                                    (schemas.Metadata.Credits != null ? '"' + schemas.Metadata.Credits + '"' : "") + "," +
                                                    '"' + schemas.Metadata.Use + '"' + "," +
                                                    (schemas.Metadata.Extent != null ? Convert.ToString(schemas.Metadata.Extent.East) : "") + "," +
                                                    (schemas.Metadata.Extent != null ? Convert.ToString(schemas.Metadata.Extent.West) : "") + "," +
                                                    (schemas.Metadata.Extent != null ? Convert.ToString(schemas.Metadata.Extent.North) : "") + "," +
                                                    (schemas.Metadata.Extent != null ? Convert.ToString(schemas.Metadata.Extent.South) : "") + "," +
                                                    schemas.Metadata.MaximumScale + "," +
                                                    schemas.Metadata.MinimumScale + "," +
                                                    schemas.SpatialReference);
                w.WriteLine(line);
                w.Flush();
            }

            return firstTime;
        }

        private bool _exportSchema(string path, JMSMLConfigETLConfigSchemaContainerSchemasSchema schema, bool isFirst)
        {
            // Export the base definition of a schema to a list of schemas

            bool firstTime = isFirst;

            using (var w = new StreamWriter(path + "\\" + _schemaListFile, !firstTime))
            {
                if (firstTime)
                {
                    w.WriteLine(_schemaHeaders);
                    w.Flush();

                    firstTime = false;
                }

                string line = string.Format("{0}",  "Schema," + 
                                                    schema.Label + "," +
                                                    schema.GeometryType + "," +
                                                    '"' + schema.LabelAlias + '"' + "," +
                                                    '"' + schema.Metadata.Label + '"' + "," +
                                                    schema.Metadata.Thumbnail + "," +
                                                    schema.Metadata.Tags + "," +
                                                    '"' + schema.Metadata.Summary + '"' + "," +
                                                    '"' + schema.Metadata.Description + '"' + "," +
                                                    (schema.Metadata.Credits != null ? '"' + schema.Metadata.Credits + '"' : "") + "," +
                                                    '"' + schema.Metadata.Use + '"' + "," +
                                                    (schema.Metadata.Extent != null ? Convert.ToString(schema.Metadata.Extent.East) : "") + "," +
                                                    (schema.Metadata.Extent != null ? Convert.ToString(schema.Metadata.Extent.West) : "") + "," +
                                                    (schema.Metadata.Extent != null ? Convert.ToString(schema.Metadata.Extent.North) : "") + "," +
                                                    (schema.Metadata.Extent != null ? Convert.ToString(schema.Metadata.Extent.South) : "") + "," +
                                                    schema.Metadata.MaximumScale + "," +
                                                    schema.Metadata.MinimumScale + "," +
                                                    schema.SpatialReference);
                w.WriteLine(line);
                w.Flush();
            }

            return firstTime;
        }

        private string _compareLines(string line1, string line2)
        {
            // Compare two comma delimited lines and return any differences

            List<string> diff;
            IEnumerable<string> set1 = line1.Split(',').Distinct();
            IEnumerable<string> set2 = line2.Split(',').Distinct();
            
            if (set2.Count() > set1.Count())
            {
                diff = set2.Except(set1).ToList();
            }
            else
            {
                diff = set1.Except(set2).ToList();
            }

            string result = "";

            foreach (string token in diff)
            {
                result = result +  " ; " + token;
            }

            if (result.Length > 0)
                result = result.TrimStart(_semicolon);
            else
                result = "OK";

            return result;
        }

        private void _writeAnnotatedSchema(string path, string folder)
        {
            // Re writes the schema in the specified file, but this time with the added test field appended.
            // Writes the results into the specified folder.

            string fileName = new FileInfo(path).Name;
            string scratchPath = folder + "\\" + _testedFilePrefix + fileName;

            StreamReader r = new StreamReader(path);
            StreamWriter w = new StreamWriter(scratchPath);

            int i = 0;
            bool bHeader = true;

            while (!r.EndOfStream)
            {
                string line = r.ReadLine();

                if (bHeader)
                {
                    line = line + "," + _testField;

                    w.WriteLine(line);
                    w.Flush();
                }
                else
                {
                    if (i < _testResults.Count())
                        line = line + "," + _testResults[i];
                    else
                        line = line + "," + "Additional line";

                    w.WriteLine(line);
                    w.Flush();
                }

                i++;
                bHeader = false;
            }

            r.Close();
            w.Close();
        }

        private void _testSchemas(string folder)
        {
            // Test exported schemas to help determine differences in output, as compared to master files.

            foreach (string file in Directory.EnumerateFiles(folder, _filePrefix + "*.csv"))
            {
                _testResults.Clear();

                string testFile = new FileInfo(file).Name;
                testFile = _testFolder + "\\" + testFile;

                StreamReader rMaster = new StreamReader(testFile);
                StreamReader rNewVersion = new StreamReader(file);

                string lineMaster;
                string lineNewVersion;

                while (!rMaster.EndOfStream && !rNewVersion.EndOfStream)
                {
                    lineMaster = rMaster.ReadLine().TrimEnd(_comma);
                    lineNewVersion = rNewVersion.ReadLine().TrimEnd(_comma);

                    _testResults.Add(_compareLines(lineMaster, lineNewVersion));
                }

                rMaster.Close();
                rNewVersion.Close();

                // Now let's write the notes out to a new version of the output.

                _writeAnnotatedSchema(file, folder + _outputFolder);
            }
        }

        public JMSMLConfigETLConfig ETLConfig
        {
            get
            {
                return _etlConfig;
            }
        }

        public void ExportSchemas(string path)
        {
            // Export each schema as a separate csv file.

            bool isFirst = true;
            string forPath = new FileInfo(path).FullName;

            JMSMLConfigETLConfigSchemaContainer container = _etlConfig.SchemaContainer;
            JMSMLConfigETLConfigSchemaContainerSchemas[] schemas = container.Schemas;

            // Export the container information

            isFirst = _exportSchemaContainer(forPath, container, isFirst);

            foreach (JMSMLConfigETLConfigSchemaContainerSchemas schemasInstance in schemas)
            {
                // Export the schema set information

                isFirst = _exportSchemaSet(forPath, schemasInstance, isFirst);

                foreach (JMSMLConfigETLConfigSchemaContainerSchemasSchema schema in schemasInstance.Schema)
                {
                    _exportFieldSchema(forPath, schema);

                    // Add this schema to the master list of schemas

                    isFirst = _exportSchema(forPath, schema, isFirst);
                }
            }

            // Now test the results against a known set of files

            //_testSchemas(forPath);
        }
    }
}