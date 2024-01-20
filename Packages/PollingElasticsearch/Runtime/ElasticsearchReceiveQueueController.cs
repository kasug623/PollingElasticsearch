using System;
using UnityEngine;

namespace kasug623.Elasticsearch.Polling
{
    public class ElasticsearchReceiveQueueController<ElasticsearchResponseType, DrawQueueType>
    {
        DrawQueueType[] drawQueue;
        int drawQueueSize;
        bool hasNewQueue = false;
        int newNumQueue = -1;
        int currentIndexNum = -1;
        int newFirstQueueIndexNum = -1;
        int newLastQueueIndexNum = -1;

        public ElasticsearchReceiveQueueController(int drawQueueSize_)
        {
            drawQueueSize = drawQueueSize_;
            drawQueue = new DrawQueueType[drawQueueSize];
        }


        // allow overwriting even the values of newly added documents in the queue.
        public void AddDocument2Queue(ElasticsearchResponseType document,
                                      ElasticsearchPollingAgent<ElasticsearchResponseType, DrawQueueType>.CountDocumentHitsHitsCountCallback countDocumentHitsHitsCount,
                                      ElasticsearchPollingAgent<ElasticsearchResponseType, DrawQueueType>.CountDocumentHitsTotalValueCallback countDocumentHitsTotalValue,
                                      ElasticsearchPollingAgent<ElasticsearchResponseType, DrawQueueType>.MappingElasticResponseToObjectCallback mappingElasticResponseToObject)
        {
            try
            {

#if APPLICATION_DEBUG
                if (countDocumentHitsHitsCount(document) != countDocumentHitsTotalValue(document))
                {
                    Debug.LogWarning("Return documents are too many"
                                        + ", so they are shaped with size in QueryDSL that is equal with DRAW_QUEUE_SIZE.");
                }
#endif

                int numNewDucuments = countDocumentHitsHitsCount(document);
                if (numNewDucuments == 0)
                {
                    hasNewQueue = false;
                    return;
                }
                else
                {
                    hasNewQueue = true;
                }

                newNumQueue = shapingNumNewDocuments(numNewDucuments);

                // update index number
                // when the index number is initialized or reset, it is set to -1.
                newFirstQueueIndexNum = (newLastQueueIndexNum + 1) % drawQueueSize;
                newLastQueueIndexNum = (newLastQueueIndexNum + newNumQueue) % drawQueueSize;

                for (int d = 0; d < newNumQueue; d++)
                {
                    currentIndexNum++;
                    currentIndexNum = currentIndexNum % drawQueueSize; // when reach a lmit of queue

                    drawQueue[currentIndexNum] = mappingElasticResponseToObject(document, d);
                }
            }
            catch (Exception e)
            {
#if APPLICATION_DEBUG
                Debug.LogException(e);
#endif
            }

        }

            private int shapingNumNewDocuments(int numNewDocuments)
        {

            // QueryDSL already shape the number of new documents to some extent.
            // So this process is not effective.
            if (numNewDocuments < drawQueueSize)
            {
                return numNewDocuments;
            }
            else
            {
#if APPLICATION_DEBUG
                Debug.LogWarning("New documents from elasticsearch exceeded a size of temporary queue.\n"
                                    + "The exceeded data was ignored.");
#endif
                return drawQueueSize;
            }
        }

        public (DrawQueueType[], bool, int, int, int) GetLastEnqueuedDataGroup()
        {
            bool currentHasNewQueue = hasNewQueue;
            hasNewQueue= false;
            return (drawQueue, currentHasNewQueue, newFirstQueueIndexNum, newNumQueue, drawQueueSize);
        }

    }
}