using Our.Umbraco.TagHelpers.Enums;

namespace Our.Umbraco.TagHelpers.Configuration
{
    public class OurUmbracoTagHelpersConfiguration
    {
        public InlineSvgTagHelperConfiguration OurSVG { get; set; } = new InlineSvgTagHelperConfiguration();
        public ImgTagHelperConfiguration OurImg { get; set; } = new ImgTagHelperConfiguration();

        public SelfHostTagHelperConfiguration OurSelfHost { get; set; } = new SelfHostTagHelperConfiguration();
    }

    public class InlineSvgTagHelperConfiguration
    {
        public bool EnsureViewBox { get; set; } = false;
        public bool Cache { get; set; } = false;
        public int CacheMinutes { get; set; } = 180;
    }

    public class ImgTagHelperConfiguration
    {
        /// <summary>
        /// Define the typical responsive breakpoints (S,M,L,XL,XXL) in which your website uses during screen resize
        /// </summary>
        public MediaQuerySizes MediaQueries { get; set; } = new MediaQuerySizes();

        /// <summary>
        /// If true, let the browser handle image lazy loading, otherwise disable to use a 3rd party JavaScript based library
        /// </summary>
        public bool UseNativeLazyLoading { get; set; } = true;

        /// <summary>
        /// Applicable if UseNativeLazyLoading is false
        /// </summary>
        public string LazyLoadCssClass { get; set; } = "lazyload";

        /// <summary>
        /// Applicable if UseNativeLazyLoading is false
        /// </summary>
        public ImagePlaceholderType LazyLoadPlaceholder { get; set; } = ImagePlaceholderType.SVG;

        /// <summary>
        /// Applicable if UseNativeLazyLoading is false & LazyLoadPlaceholder is LowQualityImage
        /// </summary>
        public int LazyLoadPlaceholderLowQualityImageQuality { get; set; } = 5;
        public bool ApplyAspectRatio { get; set; } = false;
        public bool MobileFirst { get; set; } = true;

        /// <summary>
        /// The property alias of the media type containing the alternative text value.
        /// </summary>
        public string AlternativeTextMediaTypePropertyAlias { get; set; } = "alternativeText";
    }
    public class MediaQuerySizes
    {
        public int MobileSmall { get; set; } = 320;
        public int Mobile { get; set; } = 375;
        public int MobileLarge { get; set; } = 425;
        public int TabletSmall { get; set; } = 600;
        public int Tablet { get; set; } = 768;
        public int TabletLarge { get; set; } = 980;
        public int DesktopSmall { get; set; } = 1024;
        public int Desktop { get; set; } = 1280;
        public int DesktopLarge { get; set; } = 1440;
        public int DesktopXLarge { get; set; } = 1920;
        public int DesktopXXLarge { get; set; } = 2200;
    }

    public class SelfHostTagHelperConfiguration
    {
        public string RootFolder { get; set; } = "/assets";
    }
}
