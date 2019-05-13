# Leet Crawler
### A simple asynchronous crawler for [tretton37.com](https://tretton37.com/)
Console application targeting .net core 2.2, no aditional dependencies

<img src='https://raw.githubusercontent.com/sekhargullapalli/leetcrawlerdata/master/assets/i/join.jpg' width='800' height='200'/>

# Output
The content collected by crawiling is available at repository [sekhargullapalli/leetcrawlerdata](https://github.com/sekhargullapalli/leetcrawlerdata)

# Application
## Some Crawling Assumptions
* The crawler aims at tretton37.com and is not general. 
* Only relative links identified by Regex matches considered
* Linked routes are saved as html files
* External links, dynamic links, mail addresses etc are not considered
* The 404 page at tretton37.com comes with a status code 200 Ok.This was handled explicity

## Code Organizing
* `UrlReader` for reading text content of a given route and extract links
* `ProjectExtensions` for a few extensions
* `Crawler` has an asyncronous recursive crawler

## Asyncronous Strategy Used
Starting from the index route, the recursive function `CreateRouteCrawler(string Route)` creates a route crawing recursive task and adds it to a concurrent queue type. Further tasks created during the recursive calls are also added to this queue. The `Crawl()` function then dequeue's the tasks and waits them.

These tasks collect the content (text/html) of the route and logs them in a concurrent dictionary for saving later. If the route is logged in the dictionary, it will not be crawled again. A similar approach is applied for other resources such as images, block blobs in Azure storage, style sheets and script files.

Once the routes and resources are identified, they are a handled using `Parallel.ForEach` on their respective concurrent dictionaries. Since the content of the routes are read in previous step, those items are simply saved as html files. The resources are read from their respective urls and saved with the apporpiate extensions (block blobs are saved as png files).  

## Some Challenges/ Learnings
* Simultaneous downloading of resources while crawling resulted several IO exceptions, which I did not dig into. First logging the resources and then using `Parallel.ForEach` helped me shown status and without any exceptions
* I have not benchmarked the application with a syncronous version, nor tried using standard dictionaries instead of concurrent ones. It seems that task parallelism might be advantageous for recursive operations such as this, provided the unit operations are heavy enough

## Me
Please feel free to contact at [sekhargullapalli@gmail.com](mailto:\\sekhargullapalli@gmail.com) for further discussion



