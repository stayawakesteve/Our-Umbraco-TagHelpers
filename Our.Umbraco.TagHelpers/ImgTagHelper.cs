﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Our.Umbraco.TagHelpers.Classes;
using Our.Umbraco.TagHelpers.Configuration;
using Our.Umbraco.TagHelpers.Enums;
using Our.Umbraco.TagHelpers.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Our.Umbraco.TagHelpers
{
    /// <summary>
    /// This allows you to set some PageSpeed Insights friendly optimisations to your img tags
    /// including lazy loading placeholders, dynamic alt text, aspect ratios 
    /// & image quality (for non-media items, but still on the file system)
    /// </summary>
    [HtmlTargetElement("our-img")]
    public class ImgTagHelper(IOptions<OurUmbracoTagHelpersConfiguration> globalSettings, IMediaService mediaService) : TagHelper
    {
        private OurUmbracoTagHelpersConfiguration _globalSettings = globalSettings.Value;
        private IMediaService _mediaService = mediaService;

        /// <summary>
        /// A filepath to an image on disk such as /assets/image.jpg, external URL's can also be used with limited functionality
        /// NOTE: You cannot use this in conjuction with the media-item attribute
        /// </summary>
        [HtmlAttributeName("src")]
        public string? FileSource { get; set; }

        /// <summary>
        /// An IPublishedContent Umbraco Media Item that has an .svg file extension
        /// NOTE: You cannot use this in conjuction with the src attribute
        /// </summary>
        [HtmlAttributeName("media-item")]
        public MediaWithCrops? MediaItem { get; set; }

        [HtmlAttributeName("alt")]
        public string? ImgAlt { get; set; }

        [HtmlAttributeName("width")]
        public int ImgWidth { get; set; }

        [HtmlAttributeName("width--mobile-small")]
        public int ImgWidthMobileSmall { get; set; }

        [HtmlAttributeName("width--mobile")]
        public int ImgWidthMobile { get; set; }

        [HtmlAttributeName("width--mobile-large")]
        public int ImgWidthMobileLarge { get; set; }

        [HtmlAttributeName("width--tablet-small")]
        public int ImgWidthTabletSmall { get; set; }

        [HtmlAttributeName("width--tablet")]
        public int ImgWidthTablet { get; set; }

        [HtmlAttributeName("width--tablet-large")]
        public int ImgWidthTabletLarge { get; set; }

        [HtmlAttributeName("width--desktop-small")]
        public int ImgWidthDesktopSmall { get; set; }

        [HtmlAttributeName("width--desktop")]
        public int ImgWidthDesktop { get; set; }

        [HtmlAttributeName("width--desktop-large")]
        public int ImgWidthDesktopLarge { get; set; }

        [HtmlAttributeName("width--desktop-xlarge")]
        public int ImgWidthDesktopExtraLarge { get; set; }

        [HtmlAttributeName("width--desktop-xxlarge")]
        public int ImgWidthDesktopExtraExtraLarge { get; set; }

        [HtmlAttributeName("height")]
        public int ImgHeight { get; set; }

        [HtmlAttributeName("height--mobile-small")]
        public int ImgHeightMobileSmall { get; set; }

        [HtmlAttributeName("height--mobile")]
        public int ImgHeightMobile { get; set; }

        [HtmlAttributeName("height--mobile-large")]
        public int ImgHeightMobileLarge { get; set; }

        [HtmlAttributeName("height--tablet-small")]
        public int ImgHeightTabletSmall { get; set; }

        [HtmlAttributeName("height--tablet")]
        public int ImgHeightTablet { get; set; }

        [HtmlAttributeName("height--tablet-large")]
        public int ImgHeightTabletLarge { get; set; }

        [HtmlAttributeName("height--desktop-small")]
        public int ImgHeightDesktopSmall { get; set; }

        [HtmlAttributeName("height--desktop")]
        public int ImgHeightDesktop { get; set; }

        [HtmlAttributeName("height--desktop-large")]
        public int ImgHeightDesktopLarge { get; set; }

        [HtmlAttributeName("height--desktop-xlarge")]
        public int ImgHeightDesktopExtraLarge { get; set; }

        [HtmlAttributeName("height--desktop-xxlarge")]
        public int ImgHeightDesktopExtraExtraLarge { get; set; }

        [HtmlAttributeName("cropalias")]
        public string? ImgCropAlias { get; set; }

        [HtmlAttributeName("cropalias--mobile-small")]
        public string? ImgCropAliasMobileSmall { get; set; }

        [HtmlAttributeName("cropalias--mobile")]
        public string? ImgCropAliasMobile { get; set; }

        [HtmlAttributeName("cropalias--mobile-large")]
        public string? ImgCropAliasMobileLarge { get; set; }

        [HtmlAttributeName("cropalias--tablet-small")]
        public string? ImgCropAliasTabletSmall { get; set; }

        [HtmlAttributeName("cropalias--tablet")]
        public string? ImgCropAliasTablet { get; set; }

        [HtmlAttributeName("cropalias--tablet-large")]
        public string? ImgCropAliasTabletLarge { get; set; }

        [HtmlAttributeName("cropalias--desktop-small")]
        public string? ImgCropAliasDesktopSmall { get; set; }

        [HtmlAttributeName("cropalias--desktop")]
        public string? ImgCropAliasDesktop { get; set; }

        [HtmlAttributeName("cropalias--desktop-large")]
        public string? ImgCropAliasDesktopLarge { get; set; }

        [HtmlAttributeName("cropalias--desktop-xlarge")]
        public string? ImgCropAliasDesktopExtraLarge { get; set; }

        [HtmlAttributeName("cropalias--desktop-xxlarge")]
        public string? ImgCropAliasDesktopExtraExtraLarge { get; set; }

        [HtmlAttributeName("style")]
        public string? ImgStyle { get; set; }

        [HtmlAttributeName("class")]
        public string? ImgClass { get; set; }

        [HtmlAttributeName("abovethefold")]
        public bool AboveTheFold { get; set; }

        protected HttpRequest? Request => ViewContext?.HttpContext?.Request;

        [ViewContext]
        public ViewContext? ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "img";

            #region Ensure that the file source doesn't produce a URL using the file protocol
            var baseUrl = $"{ViewContext?.HttpContext.Request.Scheme}://{ViewContext?.HttpContext.Request.Host}";

            if (FileSource != null && FileSource.StartsWith("http") == false)
            {
                FileSource = baseUrl + FileSource;
            }
            #endregion

            #region Can only use media-item OR src, don't render anything if both are provided
            // Can't use both properties together
            if (string.IsNullOrWhiteSpace(FileSource) == false && MediaItem is not null)
            {
                // KABOOM !
                // Can't decide which property to use to render image
                // So just render nothing...
                output.SuppressOutput();
                return;
            }
            #endregion

            var width = 0d;
            var height = 0d;
            var cssClasses = new List<string>();
            var imgSrc = string.Empty;
            var placeholderImgSrc = string.Empty;
            var jsLazyLoad = !_globalSettings.OurImg.UseNativeLazyLoading && !AboveTheFold;
            var style = ImgStyle;
            var hasLqip = _globalSettings.OurImg.LazyLoadPlaceholder.Equals(ImagePlaceholderType.LowQualityImage);
            var useWebP = _globalSettings.OurImg.UseWebP.Equals(true);

            if (MediaItem is not null)
            {
                #region Opting to use a media-item as the source image
                var media = _mediaService.GetById(MediaItem.Id); // Get the media object from the media library service
                var originalWidth = media.GetValue<double>("umbracoWidth"); // Determine the width from the originally uploaded image
                var originalHeight = media.GetValue<double>("umbracoHeight"); // Determine the height from the originally uploaded image
                width = ImgWidth > 0 ? ImgWidth : originalWidth; // If the element wasn't provided with a width property, use the width from the media object instead

                if (!string.IsNullOrEmpty(ImgCropAlias))
                {
                    // The element contains a crop alias property, so pull through a cropped version of the original image
                    // Also, calculate the height based on the given width using the crop profile so it's to scale
                    imgSrc = MediaItem.GetCropUrl(width: (int)width, cropAlias: ImgCropAlias);
                    if (hasLqip)
                    {
                        // Generate a low quality placeholder image if configured to do so
                        placeholderImgSrc = MediaItem.GetCropUrl(width: ImgWidth, cropAlias: ImgCropAlias, quality: _globalSettings.OurImg.LazyLoadPlaceholderLowQualityImageQuality);
                    }
                    var cropWidth = MediaItem.LocalCrops.GetCrop(ImgCropAlias).Width;
                    var cropHeight = MediaItem.LocalCrops.GetCrop(ImgCropAlias).Height;
                    height = cropHeight / cropWidth * width;
                }
                else
                {
                    if (ImgHeight > 0)
                    {
                        imgSrc = MediaItem.GetCropUrl(width: (int)ImgWidth, height: (int)ImgHeight);
                        if (hasLqip)
                        {
                            // Generate a low quality placeholder image if configured to do so
                            placeholderImgSrc = MediaItem.GetCropUrl(width: (int)ImgWidth, height: (int)ImgHeight, quality: _globalSettings.OurImg.LazyLoadPlaceholderLowQualityImageQuality);
                        }
                        width = ImgWidth;
                        height = ImgHeight != 0 ? ImgHeight : originalHeight / originalWidth * width;
                    }
                    else
                    {
                        // Pull through an image based on the given width and calculate the height so it's to scale.
                        imgSrc = MediaItem.GetCropUrl(width: (int)width);
                        if (hasLqip)
                        {
                            // Generate a low quality placeholder image if configured to do so
                            placeholderImgSrc = MediaItem.GetCropUrl(width: (int)width, quality: _globalSettings.OurImg.LazyLoadPlaceholderLowQualityImageQuality);
                        }
                        height = originalHeight / originalWidth * width;
                    }
                }

                #region Autogenerate alt text if unspecfied
                if (string.IsNullOrWhiteSpace(ImgAlt))
                {
                    output.Attributes.Add("alt", GetImageAltText(MediaItem));
                }
                else
                {
                    output.Attributes.Add("alt", ImgAlt);
                }
                #endregion
                
                #endregion
            }
            else if (!string.IsNullOrEmpty(FileSource))
            {
                #region Opting to use a file URL as the source image
                width = ImgWidth;
                height = ImgHeight;

                imgSrc = AddQueryToUrl(FileSource, "width", width.ToString()) + "&height=" + height.ToString();

                #region Autogenerate alt text if unspecfied
                if (string.IsNullOrWhiteSpace(ImgAlt))
                {
                    output.Attributes.Add("alt", "");
                }
                else
                {
                    output.Attributes.Add("alt", ImgAlt);
                }
                #endregion

                #region If width & height are not defined then return a basic <img> with just a src, alt & class (if provided)
                if (ImgWidth == 0 || ImgHeight == 0)
                {
                    output.Attributes.Add("src", FileSource);

                    if (cssClasses.Count > 0)
                    {
                        output.Attributes.Add("class", string.Join(" ", cssClasses));
                    }

                    output.TagName = "img";
                    return;
                }
                #endregion

                #endregion
            }

            #region Apply the aspect-ratio style if configured to do so. 
            /// Having width & height by themselves forces the image to initially load as that size during page load until a stylesheet kicks in.
            /// PageSpeed Insights requires all images to have a width & height.
            /// aspect-ratio sizes the element consistently, so the ratio of an element stays the same as it grows or shrinks.
            if (width > 0 && height > 0 && _globalSettings.OurImg.ApplyAspectRatio)
            {
                var aspectRatio = $"aspect-ratio: {width} / {height};";
                style = style?.Trim().TrimEnd(';');
                if (!string.IsNullOrEmpty(ImgStyle))
                {
                    style += ";";
                    output.Attributes.RemoveAll("style");
                }
                style += aspectRatio;
                style += "width: 100%; height: auto;";
                output.Attributes.Add("style", style);
                output.Attributes.RemoveAll("width");
                output.Attributes.RemoveAll("height");
            }
            #endregion

            #region If we're lazy loading via a JavaScript method, set a placeholder on the 'src' property and set the image to use 'data-src' instead.
            if (jsLazyLoad)
            {
                output.Attributes.Add("data-src", imgSrc);
                if (hasLqip && !string.IsNullOrEmpty(placeholderImgSrc))
                {
                    output.Attributes.Add("src", placeholderImgSrc);
                }
                else if (width > 0 && height > 0)
                {
                    output.Attributes.Add("src", $"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {width} {height}'%3E%3C/svg%3E");
                }
                cssClasses.Add(_globalSettings.OurImg.LazyLoadCssClass);
            }
            else
            {
                output.Attributes.Add("src", imgSrc);
            }
            #endregion

            #region If we're instead lazy loading via a browser method, add properties to assist the load order
            if (_globalSettings.OurImg.UseNativeLazyLoading || !jsLazyLoad)
            {
                if (AboveTheFold)
                {
                    output.Attributes.Add("loading", "eager"); // Load the image as soon as the page loads.
                    output.Attributes.Add("fetchpriority", "high"); // Prioritise the visible image on initial page load.
                }
                else
                {
                    output.Attributes.Add("loading", "lazy"); // Native browser lazy loading, deferring the loading until it reaches a certain distance from the viewport.
                    output.Attributes.Add("decoding", "async"); // Non-blocking image decoding, allowing your page content to display faster.
                    output.Attributes.Add("fetchpriority", "low"); // Downgrades the priority of the image given it's not currently visible on initial page load.
                }
            }
            #endregion

            #region Apply CSS classes
            if (!string.IsNullOrEmpty(ImgClass))
            {
                cssClasses.Add(ImgClass);
            }

            if (cssClasses.Count > 0)
            {
                output.Attributes.Add("class", string.Join(" ", cssClasses));
            }
            #endregion

            #region If multiple responsive image variants have been supplied, wrap the img element with a picture element and source elements per variant.
            // Only one image will be rendered at a given time based on the current screen width. 
            // The configuration allows us to define whether images are configured "mobile first". This simply alternates between min-width & max-width media queries.
            var imageSizes = GetImageSizes(MediaItem != null);

            // Only render a WebP alternative if the image is a JPEG or PNG
            var imageFormat = MediaItem != null ? Path.GetExtension(MediaItem.Url()) : Path.GetExtension(FileSource)?.ToLower();
            var renderWebP = useWebP && (imageFormat == ".jpg" || imageFormat == ".jpeg" || imageFormat == ".png");

            if (imageSizes?.Any() == true)
            {
                var sb = new StringBuilder();
                sb.AppendLine("<picture>");

                imageSizes = _globalSettings.OurImg.MobileFirst ? imageSizes.OrderByDescending(o => o.ScreenSize).ToList() : imageSizes.OrderBy(o => o.ScreenSize).ToList();

                #region If we're using a media item, render a WebP version for each size
                if (renderWebP)
                {
                    foreach (var size in imageSizes)
                    {
                        var minWidth = size.ScreenSize switch
                        {
                            OurScreenSize.DesktopXXLarge => _globalSettings.OurImg.MediaQueries.DesktopXXLarge,
                            OurScreenSize.DesktopXLarge => _globalSettings.OurImg.MediaQueries.DesktopXLarge,
                            OurScreenSize.DesktopLarge => _globalSettings.OurImg.MediaQueries.DesktopLarge,
                            OurScreenSize.Desktop => _globalSettings.OurImg.MediaQueries.Desktop,
                            OurScreenSize.DesktopSmall => _globalSettings.OurImg.MediaQueries.DesktopSmall,
                            OurScreenSize.TabletLarge => _globalSettings.OurImg.MediaQueries.TabletLarge,
                            OurScreenSize.Tablet => _globalSettings.OurImg.MediaQueries.Tablet,
                            OurScreenSize.TabletSmall => _globalSettings.OurImg.MediaQueries.TabletSmall,
                            OurScreenSize.MobileLarge => _globalSettings.OurImg.MediaQueries.MobileLarge,
                            OurScreenSize.Mobile => _globalSettings.OurImg.MediaQueries.Mobile,
                            OurScreenSize.MobileSmall => _globalSettings.OurImg.MediaQueries.MobileSmall,
                            _ => 0
                        };

                        double sourceHeight = 0;

                        if (MediaItem != null)
                        {
                            #region Configure crops which can be set at variant level or inherit from the crop alias defined on the main img element itself. If neither have a crop alias, then don't use crops.
                            var cropAlias = !string.IsNullOrEmpty(size.CropAlias) ?
                                size.CropAlias :
                                !string.IsNullOrEmpty(ImgCropAlias) ?
                                    ImgCropAlias :
                                    null;
                            #endregion

                            if (!string.IsNullOrEmpty(cropAlias))
                            {
                                var cropWidth = MediaItem.LocalCrops.GetCrop(cropAlias).Width;
                                var cropHeight = MediaItem.LocalCrops.GetCrop(cropAlias).Height;
                                sourceHeight = StringUtils.GetDouble(cropHeight) / StringUtils.GetDouble(cropWidth) * size.ImageWidth;

                                sb.AppendLine($"<source {(jsLazyLoad ? "data-" : "")}srcset=\"{MediaItem.GetCropUrl(width: size.ImageWidth, cropAlias: cropAlias, furtherOptions: "&format=webp")}\" {(_globalSettings.OurImg.MobileFirst ? $"{(minWidth > 0 ? $"media=\"(min-width: {minWidth}px)\"" : "" )}" : $"{(minWidth > 0 ? $"media=\"(max-width: {minWidth - 1}px)\"" : "" )}")} type=\"image/webp\" />");
                            }
                            else
                            {
                                if (size.ImageHeight > 0)
                                {
                                    sb.AppendLine($"<source {(jsLazyLoad ? "data-" : "")}srcset=\"{MediaItem.GetCropUrl(width: size.ImageWidth, height: size.ImageHeight, furtherOptions: "&format=webp")}\" {(_globalSettings.OurImg.MobileFirst ? $"{(minWidth > 0 ? $"media=\"(min-width: {minWidth}px)\"" : "" )}" : $"{(minWidth > 0 ? $"media=\"(max-width: {minWidth - 1}px)\"" : "" )}")} type=\"image/webp\" />");
                                }
                                else
                                {
                                    sb.AppendLine($"<source {(jsLazyLoad ? "data-" : "")}srcset=\"{MediaItem.GetCropUrl(width: size.ImageWidth, furtherOptions: "&format=webp")}\" {(_globalSettings.OurImg.MobileFirst ? $"{(minWidth > 0 ? $"media=\"(min-width: {minWidth}px)\"" : "" )}" : $"{(minWidth > 0 ? $"media=\"(max-width: {minWidth - 1}px)\"" : "" )}")} type=\"image/webp\" />");
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(FileSource) && ImgWidth > 0 && ImgHeight > 0)
                        {
                            sourceHeight = size.ImageHeight > 0 ? size.ImageHeight : ImgHeight / ImgWidth * size.ImageWidth;
                            var sourceUrl = AddQueryToUrl(FileSource, "width", size.ImageWidth.ToString()) + "&height=" + size.ImageHeight + "&format=webp";
                            sb.AppendLine($"<source {(jsLazyLoad ? "data-" : "")}srcset=\"{sourceUrl}\" {(_globalSettings.OurImg.MobileFirst ? $"{(minWidth > 0 ? $"media=\"(min-width: {minWidth}px)\"" : "" )}" : $"{(minWidth > 0 ? $"media=\"(max-width: {minWidth - 1}px)\"" : "" )}")} type=\"image/webp\" />");
                        }
                    }
                }
                #endregion

                foreach (var size in imageSizes)
                {
                    var minWidth = size.ScreenSize switch
                    {
                        OurScreenSize.DesktopXXLarge => _globalSettings.OurImg.MediaQueries.DesktopXXLarge,
                        OurScreenSize.DesktopXLarge => _globalSettings.OurImg.MediaQueries.DesktopXLarge,
                        OurScreenSize.DesktopLarge => _globalSettings.OurImg.MediaQueries.DesktopLarge,
                        OurScreenSize.Desktop => _globalSettings.OurImg.MediaQueries.Desktop,
                        OurScreenSize.DesktopSmall => _globalSettings.OurImg.MediaQueries.DesktopSmall,
                        OurScreenSize.TabletLarge => _globalSettings.OurImg.MediaQueries.TabletLarge,
                        OurScreenSize.Tablet => _globalSettings.OurImg.MediaQueries.Tablet,
                        OurScreenSize.TabletSmall => _globalSettings.OurImg.MediaQueries.TabletSmall,
                        OurScreenSize.MobileLarge => _globalSettings.OurImg.MediaQueries.MobileLarge,
                        OurScreenSize.Mobile => _globalSettings.OurImg.MediaQueries.Mobile,
                        OurScreenSize.MobileSmall => _globalSettings.OurImg.MediaQueries.MobileSmall,
                        _ => 0
                    };

                    double sourceHeight = 0;

                    if (MediaItem != null)
                    {
                        #region Configure crops which can be set at variant level or inherit from the crop alias defined on the main img element itself. If neither have a crop alias, then don't use crops.
                        var cropAlias = !string.IsNullOrEmpty(size.CropAlias) ?
                            size.CropAlias :
                            !string.IsNullOrEmpty(ImgCropAlias) ?
                                ImgCropAlias :
                                null;
                        #endregion

                        if (!string.IsNullOrEmpty(cropAlias))
                        {
                            var cropWidth = MediaItem.LocalCrops.GetCrop(cropAlias).Width;
                            var cropHeight = MediaItem.LocalCrops.GetCrop(cropAlias).Height;
                            sourceHeight = StringUtils.GetDouble(cropHeight) / StringUtils.GetDouble(cropWidth) * size.ImageWidth;

                            sb.AppendLine($"<source {(jsLazyLoad ? "data-" : "")}srcset=\"{MediaItem.GetCropUrl(width: size.ImageWidth, cropAlias: cropAlias)}\" {(_globalSettings.OurImg.MobileFirst ? $"{(minWidth > 0 ? $"media=\"(min-width: {minWidth}px)\"" : "" )}" : $"{(minWidth > 0 ? $"media=\"(max-width: {minWidth - 1}px)\"" : "" )}")} />");
                        }
                        else
                        {
                            if (size.ImageHeight > 0)
                            {
                                sb.AppendLine($"<source {(jsLazyLoad ? "data-" : "")}srcset=\"{MediaItem.GetCropUrl(width: size.ImageWidth, height: size.ImageHeight)}\" {(_globalSettings.OurImg.MobileFirst ? $"{(minWidth > 0 ? $"media=\"(min-width: {minWidth}px)\"" : "" )}" : $"{(minWidth > 0 ? $"media=\"(max-width: {minWidth - 1}px)\"" : "" )}")} />");
                            }
                            else
                            {
                                sb.AppendLine($"<source {(jsLazyLoad ? "data-" : "")}srcset=\"{MediaItem.GetCropUrl(width: size.ImageWidth)}\" {(_globalSettings.OurImg.MobileFirst ? $"{(minWidth > 0 ? $"media=\"(min-width: {minWidth}px)\"" : "" )}" : $"{(minWidth > 0 ? $"media=\"(max-width: {minWidth - 1}px)\"" : "" )}")} />");
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(FileSource) && ImgWidth > 0 && ImgHeight > 0)
                    {
                        sourceHeight = size.ImageHeight > 0 ? size.ImageHeight : ImgHeight / ImgWidth * size.ImageWidth;
                        var sourceUrl = AddQueryToUrl(FileSource, "width", size.ImageWidth.ToString()) + "&height=" + size.ImageHeight;
                        sb.AppendLine($"<source {(jsLazyLoad ? "data-" : "")}srcset=\"{sourceUrl}\" {(_globalSettings.OurImg.MobileFirst ? $"{(minWidth > 0 ? $"media=\"(min-width: {minWidth}px)\"" : "" )}" : $"{(minWidth > 0 ? $"media=\"(max-width: {minWidth - 1}px)\"" : "" )}")} />");
                    }
                }
                
                output.PreElement.SetHtmlContent(sb.ToString());
                output.PostElement.SetHtmlContent("</picture>");
            }
            #endregion
        }

        #region Private Methods
        private string GetImageAltText(IPublishedContent image)
        {
            try
            {
                if (image == null) throw new Exception("image is null");

                var alias = _globalSettings.OurImg.AlternativeTextMediaTypePropertyAlias;

                if (image.HasProperty(alias) && image.HasValue(alias))
                {
                    return image.Value<string>(alias);
                }
            }
            catch (Exception)
            {

            }

            return "";
        }
        private string AddQueryToUrl(string url, string key, string value)
        {
            Uri uri = null!;
            if (url.Contains("://"))
            {
                uri = new Uri(url);
            }
            else
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri!))
                {
                    if (Request != null)
                    {
                        uri = new Uri(new Uri(Request.GetDisplayUrl()), url);
                    }
                }
            }

            if (uri == null) {
                return url;
            }

            var baseUri = uri.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);
            var query = QueryHelpers.ParseQuery(uri.Query);

            var items = query.SelectMany(x => x.Value, (col, value) => new KeyValuePair<string, string>(col.Key, value ?? string.Empty)).ToList();

            items.RemoveAll(x => x.Key == key);

            var qb = new QueryBuilder(items);

            qb.Add(key, value);

            return baseUri + qb.ToQueryString();
        }
        private List<OurImageSize> GetImageSizes(bool isMedia = true)
        {
            var imageSizes = new List<OurImageSize>();

            if(ImgWidth > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.Default, ImgWidth, ImgHeight, ImgCropAlias) : new OurImageSize(Enums.OurScreenSize.Default, ImgWidth, ImgHeight));
            }
            if(ImgWidthMobileSmall > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.MobileSmall, ImgWidthMobileSmall, ImgHeightMobileSmall, ImgCropAliasMobileSmall) : new OurImageSize(Enums.OurScreenSize.MobileSmall, ImgWidthMobileSmall, ImgHeightMobileSmall));
            }
            if(ImgWidthMobile > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.Mobile, ImgWidthMobile, ImgHeightMobile, ImgCropAliasMobile) : new OurImageSize(Enums.OurScreenSize.Mobile, ImgWidthMobile, ImgHeightMobile));
            }
            if(ImgWidthMobileLarge > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.MobileLarge, ImgWidthMobileLarge, ImgHeightMobileLarge, ImgCropAliasMobileLarge) : new OurImageSize(Enums.OurScreenSize.MobileLarge, ImgWidthMobileLarge, ImgHeightMobileLarge));
            }
            if(ImgWidthTabletSmall > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.TabletSmall, ImgWidthTabletSmall, ImgHeightTabletSmall, ImgCropAliasTabletSmall) : new OurImageSize(Enums.OurScreenSize.TabletSmall, ImgWidthTabletSmall, ImgHeightTabletSmall));
            }
            if(ImgWidthTablet > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.Tablet, ImgWidthTablet, ImgHeightTablet, ImgCropAliasTablet) : new OurImageSize(Enums.OurScreenSize.Tablet, ImgWidthTablet, ImgHeightTablet));
            }
            if(ImgWidthTabletLarge > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.TabletLarge, ImgWidthTabletLarge, ImgHeightTabletLarge, ImgCropAliasTabletLarge) : new OurImageSize(Enums.OurScreenSize.TabletLarge, ImgWidthTabletLarge, ImgHeightTabletLarge));
            }
            if(ImgWidthDesktopSmall > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.DesktopSmall, ImgWidthDesktopSmall, ImgHeightDesktopSmall, ImgCropAliasDesktopSmall) : new OurImageSize(Enums.OurScreenSize.DesktopSmall, ImgWidthDesktopSmall, ImgHeightDesktopSmall));
            }
            if(ImgWidthDesktop > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.Desktop, ImgWidthDesktop, ImgHeightDesktop, ImgCropAliasDesktop) : new OurImageSize(Enums.OurScreenSize.Desktop, ImgWidthDesktop, ImgHeightDesktop));
            }
            if(ImgWidthDesktopLarge > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.DesktopLarge, ImgWidthDesktopLarge, ImgHeightDesktopLarge, ImgCropAliasDesktopLarge) : new OurImageSize(Enums.OurScreenSize.DesktopLarge, ImgWidthDesktopLarge, ImgHeightDesktopLarge));
            }
            if(ImgWidthDesktopExtraLarge > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.DesktopXLarge, ImgWidthDesktopExtraLarge, ImgHeightDesktopExtraLarge, ImgCropAliasDesktopExtraLarge) : new OurImageSize(Enums.OurScreenSize.DesktopXLarge, ImgWidthDesktopExtraLarge, ImgHeightDesktopExtraLarge));
            }
            if(ImgWidthDesktopExtraExtraLarge > 0)
            {
                imageSizes.Add(isMedia ? new OurImageSize(Enums.OurScreenSize.DesktopXXLarge, ImgWidthDesktopExtraExtraLarge, ImgHeightDesktopExtraExtraLarge, ImgCropAliasDesktopExtraExtraLarge) : new OurImageSize(Enums.OurScreenSize.DesktopXXLarge, ImgWidthDesktopExtraExtraLarge, ImgHeightDesktopExtraExtraLarge));
            }

            return imageSizes;
        }
        #endregion
    }
}