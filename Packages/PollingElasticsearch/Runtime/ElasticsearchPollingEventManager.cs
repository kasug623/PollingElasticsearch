using System;

namespace kasug623.Elasticsearch.Polling
{
    public class CustomEventArgs<ElasticsearchResponseType> : EventArgs
    {
        public ElasticsearchResponseType CustomData { get; set; }

        public CustomEventArgs(){}

        public CustomEventArgs(ElasticsearchResponseType customData)
        {
            CustomData = customData;
        }
    }

    public class ElasticsearchPollingEventHandler<ElasticsearchResponseType>
    {

        public delegate void EventHandler(object sender, CustomEventArgs<ElasticsearchResponseType> eventInfo);

        public event EventHandler events;

        // Method to raise the event
        public void RaiseEventFetchDocumentData(ElasticsearchResponseType document)
        {
            // Raise the event
            events?.Invoke(this, new CustomEventArgs<ElasticsearchResponseType>(document));
        }
    }

    public class ElasticsearchPollingEventCollector<ElasticsearchResponseType, DrawQueueType>
    {
        ElasticsearchReceiveQueueController<ElasticsearchResponseType, DrawQueueType> receiveQueueContoller;

        ElasticsearchPollingAgent<ElasticsearchResponseType, DrawQueueType>.CountDocumentHitsHitsCountCallback countDocumentHitsHitsCount;
        ElasticsearchPollingAgent<ElasticsearchResponseType, DrawQueueType>.CountDocumentHitsTotalValueCallback countDocumentHitsTotalValue;
        ElasticsearchPollingAgent<ElasticsearchResponseType, DrawQueueType>.MappingElasticResponseToObjectCallback mappingElasticResponseToObject;

        public ElasticsearchPollingEventCollector(ElasticsearchReceiveQueueController<ElasticsearchResponseType, DrawQueueType> receiveQueueContoller_,
                                                  ElasticsearchPollingAgent<ElasticsearchResponseType, DrawQueueType>.CountDocumentHitsHitsCountCallback countDocumentHitsHitsCount_,
                                                  ElasticsearchPollingAgent<ElasticsearchResponseType, DrawQueueType>.CountDocumentHitsTotalValueCallback countDocumentHitsTotalValue_,
                                                  ElasticsearchPollingAgent<ElasticsearchResponseType, DrawQueueType>.MappingElasticResponseToObjectCallback mappingElasticResponseToObject_)
        {
            receiveQueueContoller = receiveQueueContoller_;
            countDocumentHitsHitsCount = countDocumentHitsHitsCount_;
            countDocumentHitsTotalValue = countDocumentHitsTotalValue_;
            mappingElasticResponseToObject = mappingElasticResponseToObject_;  
        }

        public void parseDocument(object sender, CustomEventArgs<ElasticsearchResponseType> eventInfo)
        {
            receiveQueueContoller.AddDocument2Queue(eventInfo.CustomData,
                                                    countDocumentHitsHitsCount,
                                                    countDocumentHitsTotalValue,
                                                    mappingElasticResponseToObject);
        }
    }

}