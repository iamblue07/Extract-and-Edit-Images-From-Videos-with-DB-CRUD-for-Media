using BDM_P.Data;
using BDM_P.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;

namespace BDM_P.Services
{
    public class SemanticSearchService
    {
        private readonly ProcessedService _processedService = new ProcessedService();
        private readonly UnprocessedService _unprocessedService = new UnprocessedService();

        public List<SemanticSearchResult> FindSimilarImages(byte[] queryImage, double threshold = 50.0)
        {
            var results = new List<SemanticSearchResult>();

            try
            {
                using (var conn = Db.GetConn())
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "fnFindSimilarImagesByBlob";

                    // Add input parameters
                    var queryParam = new OracleParameter("p_query_blob", OracleDbType.Blob)
                    {
                        Direction = ParameterDirection.Input,
                        Value = queryImage
                    };
                    cmd.Parameters.Add(queryParam);

                    var thresholdParam = new OracleParameter("p_threshold", OracleDbType.Double)
                    {
                        Direction = ParameterDirection.Input,
                        Value = threshold
                    };
                    cmd.Parameters.Add(thresholdParam);

                    // Add return parameter for the ref cursor
                    var resultParam = new OracleParameter("result", OracleDbType.RefCursor)
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(resultParam);

                    // Execute the function
                    cmd.ExecuteNonQuery();

                    // Get the ref cursor result
                    var refCursor = (OracleRefCursor)resultParam.Value;
                    using (var reader = refCursor.GetDataReader())
                    {
                        while (reader.Read())
                        {
                            // Skip error rows
                            if (reader.GetString(1) == "ERROR")
                                continue;

                            var result = new SemanticSearchResult
                            {
                                Id = reader.GetInt32(0),
                                ImageType = reader.GetString(1),
                                VideoId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                SimilarityScore = reader.GetDouble(3)
                            };

                            // Get the actual image data
                            try
                            {
                                if (result.ImageType == "PROCESSED")
                                {
                                    result.ImageData = _processedService.GetImageById(result.Id);
                                }
                                else
                                {
                                    result.ImageData = _unprocessedService.GetImageById(result.Id);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Trace.TraceError($"Error getting image data for ID {result.Id}: {ex.Message}");
                                // Continue with other results even if one image fails to load
                            }

                            results.Add(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Semantic search error: {ex.Message}");
                throw new Exception("Semantic search failed: " + ex.Message, ex);
            }

            return results;
        }

        // Alternative direct SQL approach if the function still has issues
        public List<SemanticSearchResult> FindSimilarImagesDirect(byte[] queryImage, double threshold = 50.0)
        {
            var results = new List<SemanticSearchResult>();

            try
            {
                using (var conn = Db.GetConn())
                {
                    // Use a direct PL/SQL block call
                    string sql = @"
                        DECLARE
                            v_cursor SYS_REFCURSOR;
                        BEGIN
                            v_cursor := fnFindSimilarImagesByBlob(:query_blob, :threshold);
                            :result := v_cursor;
                        END;";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("query_blob", OracleDbType.Blob).Value = queryImage;
                        cmd.Parameters.Add("threshold", OracleDbType.Double).Value = threshold;

                        var resultParam = new OracleParameter("result", OracleDbType.RefCursor)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(resultParam);

                        cmd.ExecuteNonQuery();

                        using (var reader = ((OracleRefCursor)resultParam.Value).GetDataReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.GetString(1) == "ERROR")
                                    continue;

                                var result = new SemanticSearchResult
                                {
                                    Id = reader.GetInt32(0),
                                    ImageType = reader.GetString(1),
                                    VideoId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                    SimilarityScore = reader.GetDouble(3)
                                };

                                // Get image data
                                if (result.ImageType == "PROCESSED")
                                {
                                    result.ImageData = _processedService.GetImageById(result.Id);
                                }
                                else
                                {
                                    result.ImageData = _unprocessedService.GetImageById(result.Id);
                                }

                                results.Add(result);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Direct semantic search error: {ex.Message}");
                throw new Exception("Semantic search failed: " + ex.Message, ex);
            }

            return results;
        }
    }
}