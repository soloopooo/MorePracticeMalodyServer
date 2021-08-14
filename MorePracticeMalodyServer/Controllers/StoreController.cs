﻿/* Map Store Controller of MorePractice Malody Server. 
 * Make sure turning off the DEBUG mode.
 * 
 * Just provide a valid sid and cid, and the server will 
 * return the charts.
 * Known bugs: 
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MorePracticeMalodyServer.Data;
using MorePracticeMalodyServer.Model;
using MorePracticeMalodyServer.Model.DataModel;
using MorePracticeMalodyServer.Model.DbModel;

namespace MorePracticeMalodyServer.Controllers
{
    /// <summary>
    ///     Map Store Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IMemoryCache cache;
        private readonly DataContext context;
        private readonly ILogger<StoreController> logger;

        public StoreController(ILogger<StoreController> logger, DataContext context, IMemoryCache cache)
        {
            this.logger = logger;
            this.context = context;
            this.cache = cache;
        }

        /// <summary>
        ///     Get a list of charts under the specified query conditions.
        /// </summary>
        /// <param name="uid">User id</param>
        /// <param name="api">Api version</param>
        /// <param name="word">Search keyword</param>
        /// <param name="org">Whether to return the original title</param>
        /// <param name="mode">Returns the chart of the specified mode</param>
        /// <param name="lvge">Returns the chart whose level is greater than this value</param>
        /// <param name="lvle">Returns the chart whose level is less than this value</param>
        /// <param name="beta">Return to non-stable chart</param>
        /// <param name="from">Paging start</param>
        /// <returns>A warped json list of song infos.</returns>
        [Route("list")]
        [HttpGet]
        public async Task<Response<SongInfo>> GetList(int uid, int api, string word, int org, int mode, int lvge,
            int lvle, int beta, int from)
        {
            // If not support the api version, throw a exception.
            if (api != Consts.API_VERSION)
                throw new NotSupportedException($"This server not support api version {api}.");

            var resp = new Response<SongInfo>();
            // Max Items server will return.
            var maxItem = 50;

            // Try to find data from memory cache first.
            if (cache.TryGetValue(word.ToLower(), out List<Song> cachedSongList))
            {
            }
            else // Query db and write result to cache
            {
                var result = context.Songs
                    .Where(s => s.Title.Contains(word.ToLower()) || s.OriginalTitle.Contains(word.ToLower()))
                    .AsNoTracking()
                    .ToList();

                // Create new cache entry. Set value abd expiration.
                var cacheEntry = cache.CreateEntry(word);
                cacheEntry.Value = result;
                cacheEntry.AbsoluteExpirationRelativeToNow = new TimeSpan(0, 2, 0);

                cachedSongList = result;
            }

            resp.Code = 0;

            // Filter cached song to match query string.
            foreach (var song in cachedSongList)
            {
                if (beta == 0) // Hide all Beta and Alpha chart if don't want to see beta.
                    song.Charts = song.Charts
                        .Where(c => c.Type == ChartState.Stable)
                        .ToList();

                song.Charts = song.Charts
                    .Where(c => c.Level > lvge && c.Level < lvle) // select level
                    .ToList();

                if (mode != -1)
                    song.Charts = song.Charts
                        .Where(c => c.Mode == mode) // select mode
                        .ToList();
            }

            // Delete all song if it don't have any chart match query string.
            cachedSongList = cachedSongList
                .Where(s => s.Charts.Any())
                .ToList();

            // If has more than MaxItem left, set HasMore to true, and set next index.
            if (cachedSongList.Count - from * maxItem > maxItem)
            {
                resp.HasMore = true;
                resp.Next = from + 1;
            }
            else
            {
                resp.HasMore = false;
            }

            // Init SongInfo and write data to it.
            Parallel.For(from * maxItem,
                from * maxItem - cachedSongList.Count > maxItem ? from * maxItem + maxItem : cachedSongList.Count,
                index =>
                {
                    resp.Data.Add(new SongInfo
                    {
                        Artist = cachedSongList[index].Artist,
                        Bpm = cachedSongList[index].Bpm,
                        Cover = cachedSongList[index].Cover,
                        Length = cachedSongList[index].Length,
                        Mode = cachedSongList[index].Mode,
                        Sid = cachedSongList[index].SongId,
                        Time = GetTimeStamp(cachedSongList[index].Time),
                        Title = org == 0 ? cachedSongList[index].Title : cachedSongList[index].OriginalTitle
                    });
                });

            return resp;
        }

        /// <summary>
        ///     Get a list of promoted song.
        /// </summary>
        /// <param name="uid">User id</param>
        /// <param name="api">Api version</param>
        /// <param name="org">Whether to return the original title</param>
        /// <param name="mode">Returns the chart of the specified mode</param>
        /// <param name="from">Paging start</param>
        /// <returns>A warped json list of song infos.</returns>
        [Route("promote")]
        [HttpGet]
        public async Task<Response<SongInfo>> GetPromote(int uid, int api, int org, int mode, int from)
        {
            // If not support the api version, throw a exception.
            if (api != Consts.API_VERSION)
                throw new NotSupportedException($"This server not support api version {api}.");

            //TODO
            return new Response<SongInfo>();
        }

        /// <summary>
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="api"></param>
        /// <param name="sid"></param>
        /// <param name="beta"></param>
        /// <param name="mode"></param>
        /// <param name="from"></param>
        /// <returns name="resp"></returns>
        [Route("charts")]
        [HttpGet]
        public async Task<Response<ChartInfo>> GetChart(int uid, int api, int sid, int beta, int mode, int from)
        {
            // If not support the api version, throw a exception.
            if (api != Consts.API_VERSION)
                throw new NotSupportedException($"This server not support api version {api}.");

            var resp = new Response<ChartInfo>();
            // Max Items server will return.
            var maxItem = 50;

            // Try to find song with correct id.
            try
            {
                var result = await context.Songs
                    .AsNoTracking()
                    .FirstAsync(s => s.SongId == sid);
                // Success
                resp.Code = 0;
                var charts = result.Charts.ToList();

                // Select stable if don't want to see beta.
                if (beta == 0)
                    result.Charts = charts
                        .Where(c => c.Type == ChartState.Stable)
                        .ToList();

                // Select mode if give a mode.
                if (mode != -1)
                    result.Charts = charts
                        .Where(c => c.Mode == mode)
                        .ToList();

                // If has more than MaxItem left, set HasMore to true, and set next index.
                if (charts.Count - from * maxItem > maxItem)
                {
                    resp.HasMore = true;
                    resp.Next = from + 1;
                }
                else
                {
                    resp.HasMore = false;
                }

                // Add charts to resp.
                for (var i = from * maxItem; resp.HasMore ? i != maxItem : i != charts.Count; i++)
                    resp.Data.Add(new ChartInfo
                    {
                        Cid = charts[i].ChartId,
                        Creator = charts[i].Creator,
                        Length = charts[i].Length,
                        Level = charts[i].Level,
                        Mode = charts[i].Mode,
                        Size = charts[i].Size,
                        Type = charts[i].Type,
                        Uid = charts[i].UserId,
                        Version = charts[i].Version
                    });
            }
            catch (InvalidOperationException) // Song doesn't exist.
            {
                logger.LogError("Could not find Song with Id {id}", sid);
#if DEBUG
                throw;
#else
                return resp;
#endif
            }

            return resp;
        }

        /// <summary>
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="api"></param>
        /// <param name="sid"></param>
        /// <param name="cid"></param>
        /// <param name="org"></param>
        /// <returns name="resp"></returns>
        [Route("query")]
        [HttpGet]
        public async Task<Response<SongInfo>> QuerySong(int uid, int api, int sid = -1, int cid = -1, int org = 0)
        {
            // If not support the api version, throw a exception.
            if (api != Consts.API_VERSION)
                throw new NotSupportedException($"This server does not support api version {api}.");

            // If not providing a valid SID or CID, throw a exception.
            if (sid == -1 && cid == -1)
            {
                logger.LogError("Query for sid {sid} or cid {cid} is not valid!", sid, cid);
                throw new ArgumentException("A valid SID or CID not provided.");
            }

            //
            var resp = new Response<SongInfo>();

            if (sid != -1) // Use sid to select song
                try
                {
                    var result = await context.Songs
                        .AsNoTracking()
                        .FirstAsync(s => s.SongId == sid);

                    // Success.
                    resp.Code = 0;
                    resp.Data.Add(new SongInfo // Add song to response.
                    {
                        Sid = result.SongId,
                        Artist = result.Artist,
                        Bpm = result.Bpm,
                        Cover = result.Cover,
                        Length = result.Length,
                        Mode = result.Mode,
                        Time = GetTimeStamp(result.Time),
                        Title = org != 0 ? result.OriginalTitle : result.Title
                    });

                    return resp;
                }
                catch (InvalidOperationException e) // Song doesn't exist.
                {
                    logger.LogError("Could not find a song with sid {sid}", sid);
#if DEBUG
                    throw;
#else
                    return resp;
#endif
                }

            if (cid != -1)
                try
                {
                    var result = await context.Charts
                        .AsNoTracking()
                        .FirstAsync(c => c.ChartId == cid);

                    resp.Code = 0;

                    var song = result.Song;

                    resp.Data.Add(new SongInfo
                    {
                        Artist = song.Artist,
                        Bpm = song.Bpm,
                        Cover = song.Cover,
                        Length = song.Length,
                        Mode = song.Mode,
                        Sid = song.SongId,
                        Time = GetTimeStamp(song.Time),
                        Title = org != 0 ? song.OriginalTitle : song.Title
                    });

                    return resp;
                }
                catch (InvalidOperationException e)
                {
                    logger.LogError("Could not find a song with cid {cid}", cid);
#if DEBUG
                    throw;
#else
                    return resp;
#endif
                }
#if DEBUG
            logger.LogCritical("WTF goes here? Check QuerySong() for some sucking detail.");
            return resp;
#else
            return resp;
#endif
        }

        /// <summary>
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="api"></param>
        /// <param name="cid"></param>
        /// <returns name="resp"></returns>
        [Route("download")]
        [HttpGet]
        public async Task<DownloadResponse> GetDownload(int uid, int api, int cid)
        {
            // If not support the api version, throw a exception.
            if (api != Consts.API_VERSION)
                throw new NotSupportedException($"This server does not support api version {api}.");

            var resp = new DownloadResponse();
            Chart chart = null;

            // Try to find the chart with cid given.
            try
            {
                chart = await context.Charts
                    .AsNoTracking()
                    .FirstAsync(c => c.ChartId == cid);
            }
            catch (InvalidOperationException) // No chart found.
            {
                logger.LogWarning("No chart with cid {cid} find!", cid);
                // Set response code to -2, indicates no chart found.
                resp.Code = -2;
                return resp;
            }

            // Try to find the download records with chart.
            try
            {
                var dls = context.Downloads
                    .Where(d => d.Chart == chart)
                    .AsNoTracking()
                    .ToList();

                if (dls.Any())
                {
                    resp.Code = 0;
                    resp.Sid = chart.Song.SongId;
                    resp.Cid = chart.ChartId;
                    foreach (var dl in dls)
                        resp.Items.Add(new DownloadInfo
                        {
                            File = dl.File,
                            Hash = dl.Hash,
                            Name = dl.Name
                        });

                    return resp;
                }
            }
            catch (ArgumentNullException e) // Something wrong because chart is null. This is impossible.
            {
                logger.LogError(e, "Query downloads for chart {cid} meets something wired.");
#if DEBUG
                throw;
#else
                resp.Code = -2;
                return resp;
#endif
            }

            return resp;
        }

        /// <summary>
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="api"></param>
        /// <param name="active"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        [Route("events")]
        [HttpGet]
        public async Task<Response<EventInfo>> GetEvent(int uid, int api, int active, int from)
        {
            // If not support the api version, throw a exception.
            if (api != Consts.API_VERSION)
                throw new NotSupportedException($"This server does not support api version {api}.");

            var maxItem = 50;
            var resp = new Response<EventInfo>();

            try
            {
                // Try query events.
                var query = context.Events
                    .AsNoTracking();

                if (active == 1)
                    query = query.Where(e => e.Active);

                // Success.
                var result = query.ToList(); // TODO: Save events to cache?

                resp.Code = 0;
                // To see if has more to send.
                if (result.Count - maxItem * from > maxItem)
                {
                    resp.HasMore = true;
                    resp.Next = from + 1;
                }
                else
                {
                    resp.HasMore = false;
                }

                // Write to response.Data
                for (var i = maxItem * from; resp.HasMore ? i != maxItem : i != result.Count; i++)
                    resp.Data.Add(new EventInfo
                    {
                        Active = result[i].Active,
                        Cover = result[i].Cover,
                        Eid = result[i].EventId,
                        End = result[i].End.ToString("yyyy-mm-dd"),
                        Name = result[i].Name,
                        Start = result[i].Start.ToString("yyyy-mm-dd")
                    });

                return resp;
            }
            catch (ArgumentNullException) // Something impossible happened.
            {
                logger.LogError("?");
                throw;
                // I DONT WANT TO WRITE HERE ANY MORE.
            }

            // Also something impossible.
            return resp;
        }

        private long GetTimeStamp()
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            var nowTime = DateTime.Now;
            var unixTime = (long) Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }

        private long GetTimeStamp(DateTime time)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            var nowTime = time;
            var unixTime = (long) Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }
    }
}