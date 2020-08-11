namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Options for token acquisition service.
    /// </summary>
    public class TokenAcquisitionOptions
    {
        /// <summary>
        /// Specifies if an instance of <see cref="ITokenAcquisition"/> should be a singleton.
        /// </summary>
        public bool IsSingleton { get; set; }
    }
}
