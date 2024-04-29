namespace kasug623.Elasticsearch.Polling
{
    public abstract class ElasticsearchQueryDSL<ElasticsearchResponseType>
    {
        protected string queryDSL;

        public string GetQueryDSL()
        {
            return queryDSL;
        }
        public abstract void CreateQueryDSL();
        public abstract void UpdateQueryDSL(ElasticsearchResponseType response);

        public abstract void UpdateQueryDSL();
    }
}