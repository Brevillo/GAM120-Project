using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackSpider : GenericSpider {

    [SerializeField] private float minIdleTime, maxIdleTime, minCrawlTime, maxCrawlTime;

    [SerializeField] private CrawlParameters crawlParameters;

    protected override IEnumerator Behaviour() {

        while (true) {

            Velocity = Vector2.zero;

            yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));

            yield return Crawl(Random.Range(minCrawlTime, maxCrawlTime), Random.value > 0.5f ? -1 : 1, crawlParameters);
        }
    }
}
