using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;

namespace kasug623.Elasticsearch.Polling
{
    public class ElasticsearchPollingAgent<ElasticsearchResponseType, DrawQueueType>
    {
        ElasticsearchQueryDSL<ElasticsearchResponseType> elasticsearchQueryDSL;

        ElasticsearchReceiveQueueController<ElasticsearchResponseType, DrawQueueType> receiveQueueContoller;
        ElasticsearchPollingEventHandler<ElasticsearchResponseType> pollingEventHandler;
        ElasticsearchPollingEventCollector<ElasticsearchResponseType, DrawQueueType> pollingEventCollector;

        public delegate int CountDocumentHitsHitsCountCallback(ElasticsearchResponseType document);
        public delegate int CountDocumentHitsTotalValueCallback(ElasticsearchResponseType document);
        public delegate DrawQueueType MappingElasticResponseToObjectCallback(ElasticsearchResponseType document, int d);

        bool IsPolling = true;
        int pollingIntervalMiliSec;
        string url;
        float webRequestTimeoutDurationSec = 5.0f;

        CancellationTokenSource cancellationTokenSource;

        public ElasticsearchPollingAgent(AccessInfo accessInfo,
                                         ElasticsearchQueryDSL<ElasticsearchResponseType> elasticsearchQueryDSL_,
                                         CountDocumentHitsHitsCountCallback countDocumentHitsHitsCount_,
                                         CountDocumentHitsTotalValueCallback countDocumentHitsTotalValue_,
                                         MappingElasticResponseToObjectCallback mappingElasticResponseToObject_)
        {
            // create instances
            receiveQueueContoller = new ElasticsearchReceiveQueueController<ElasticsearchResponseType, DrawQueueType>(accessInfo.ReceiveQueueSize);
            pollingEventCollector = new ElasticsearchPollingEventCollector<ElasticsearchResponseType, DrawQueueType>(receiveQueueContoller,
                                                                                                                    countDocumentHitsHitsCount_,
                                                                                                                    countDocumentHitsTotalValue_,
                                                                                                                    mappingElasticResponseToObject_);
            pollingEventHandler = new ElasticsearchPollingEventHandler<ElasticsearchResponseType>();

            // register events
            pollingEventHandler.events += pollingEventCollector.parseDocument;

            // initialize request values
            elasticsearchQueryDSL = elasticsearchQueryDSL_;
            url = accessInfo.Url;

            // inialize polling value
            pollingIntervalMiliSec = accessInfo.PollingIntervalMiliSec;

        }

        public void StartPolling()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var ct = cancellationTokenSource.Token;
            try
            {
                polling(ct, elasticGet).Forget();
            }
            catch
            {
                Cancel().Forget();
            }
        }


        private async UniTask elasticGet(CancellationToken ct)
        {
            string postData = elasticsearchQueryDSL.GetQueryDSL();

            UnityWebRequest request = new UnityWebRequest(url, "post");
            request.SetRequestHeader("content-type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(postData));

            try
            {
                // prepare another cancel token for timeout
                // ref. https://neue.cc/2022/07/13_Cancellation.html
                var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter((int)(webRequestTimeoutDurationSec * 1000)); // start timer for timeout
                try
                {
                    await request.SendWebRequest().WithCancellation(cts.Token);    
                }
                catch (Exception e)
                {
#if POLLING_ELASTICSEARCH_DEBUG
                    // when timeout
                    Debug.LogWarning(e);
#endif
                    return;
                }

                var result = request.result;                // this process can be performed only in the main thread.
                var json = request.downloadHandler.text;    // this process can be performed only in the main thread.
                var errortext = request.error;              // this process can be performed only in the main thread.

                if (result != UnityWebRequest.Result.Success) return;

                await UniTask.RunOnThreadPool(() =>
                {
#if POLLING_ELASTICSEARCH_DEBUG
                    Profiler.BeginThreadProfiling("MyWebRequest", "DataConvert");
#endif
                    try
                    {
                        // convert string to pre-defined class
                        // jsonutility.fromjson can be used only on a main thread.
                        // so, jsonconvert.deserializeobject of newtonsoft.json is adopted.
                        // elasticresponse = jsonutility.fromjson<api.response>(json);
                        // elasticresponseoldesttime = jsonconvert.deserializeobject<api.oldesttime.response>(json);
                        ElasticsearchResponseType response = JsonConvert.DeserializeObject<ElasticsearchResponseType>(json);

                        pollingEventHandler.RaiseEventFetchDocumentData(response);
                        elasticsearchQueryDSL.UpdateQueryDSL(response);

                    }

                    catch
                    {
#if POLLING_ELASTICSEARCH_DEBUG
                        Debug.LogWarning("Failed to parse documents.\n"
                                            + "The queryDSL may not be matching the structure of the Elastic Response. "
                                            + "The HTTP Response data is below.\n"
                                            + "\n" + json + "\n");
#endif
                    }
#if POLLING_ELASTICSEARCH_DEBUG
                    Profiler.EndThreadProfiling();
#endif
                }
                ); 
            }
            catch
            {
                request.uploadHandler = null;
                request.Dispose();
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = null;
            }
            finally
            {
                request?.Dispose();
                cancellationTokenSource?.Dispose();
            }
        }

        public delegate UniTask AsyncOperationDelegate(CancellationToken ct);

        private async UniTask polling(CancellationToken ct, AsyncOperationDelegate asyncOperation)
        {
            try
            {
                while (IsPolling)
                {
                    ct.ThrowIfCancellationRequested();

                    await asyncOperation(ct);

                    ct.ThrowIfCancellationRequested();

                    await UniTask.Delay(pollingIntervalMiliSec, cancellationToken: ct);
                }

#if POLLING_ELASTICSEARCH_DEBUG
                Debug.Log("break of a polling loop");
#endif
            }
            catch (OperationCanceledException e)
            {
#if POLLING_ELASTICSEARCH_DEBUG
                Debug.LogWarning(e);
#endif
            }
            finally
            {
#if POLLING_ELASTICSEARCH_DEBUG
                Debug.Log("end of a polling");
#endif
            }

        }

            public (DrawQueueType[], bool, int, int, int) GetLastEnqueuedDataGroup()
        {
            return receiveQueueContoller.GetLastEnqueuedDataGroup();
        }

        public async UniTask Cancel()
        {
#if POLLING_ELASTICSEARCH_DEBUG
            Debug.Log("Cancel of a polling is called.");
#endif
            IsPolling = false;  // stop while loop
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            await UniTask.Delay(1000);
            cancellationTokenSource = null;
        }
    }
}