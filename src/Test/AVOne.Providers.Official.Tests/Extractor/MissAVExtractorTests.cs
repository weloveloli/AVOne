using Xunit;
using AVOne.Providers.Official.Extractor;
using System.Text.RegularExpressions;

namespace AVOne.Providers.Official.Extractor.Tests
{
    public class MissAVExtractorTests
    {
        [Fact()]
        public void MissAVExtractorTest()
        {
            var html = @"            let source

            eval(function(p,a,c,k,e,d){e=function(c){return c.toString(36)};if(!''.replace(/^/,String)){while(c--){d[c.toString(a)]=k[c]||c.toString(a)}k=[function(e){return d[e]}];e=function(){return'\\w+'};c=1};while(c--){if(k[c]){p=p.replace(new RegExp('\\b'+e(c)+'\\b','g'),k[c])}}return p}('q=\'i://k-h-g.f.e/d=c-b&a=9&8=%7-3-2-1-0%6/5-3-2-1-0/p.4\';o=\'i://k-h-g.f.e/d=c-b&a=9&8=%7-3-2-1-0%6/5-3-2-1-0/n/j.4\';m=\'i://k-h-g.f.e/d=c-b&a=9&8=%7-3-2-1-0%6/5-3-2-1-0/l/j.4\';',27,27,'6f03e237a150|9dbc|42fa|7e7f|m3u8|73e9767b|2F|2F73e9767b|token_path|1677387428|expires|7YrGcZLgGAHsO0rPZ3fWVph3kRANo8c|OYliAa_Gy0v|bcdn_token|com|thisiscdn|bbg|3325|https|video||1280x720|source1280|842x480|source842|playlist|source'.split('|'),0,{}))

            const video = document.querySelector('video.player')

            const initialPlayerEvent = () => {
                window.player.on('play', () => {
                    if (! hasPlayed) {
                        if (window.hls) {
                            window.hls.startLoad(-1)
                        }

                        hasPlayed = true

                        window.dataLayer.push({
                            event: 'videoPlay',
                            item: {
                                dvd_id: 'cus-1468',
                            },
                        })
                    }
                })

                window.player.on('enterfullscreen', () => {
                    screen.orientation.lock('landscape').catch(() => {})

                    setHlsDefaultLevel()
                })";

            var source = new MissAVExtractor(null, null).GetSources(html);

            Assert.Equal("https://bbg.3325.com/2F73e9767b/1677387428/7YrGcZLgGAHsO0rPZ3fWVph3kRANo8c/OYliAa_Gy0v/thisiscdn.com/842x480/source842/playlist.m3u8", source);
        }

        [Fact()]
        public void ExtractAsyncTest()
        {

        }

        [Fact()]
        public void SupportTest()
        {

        }
    }
}
// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.
