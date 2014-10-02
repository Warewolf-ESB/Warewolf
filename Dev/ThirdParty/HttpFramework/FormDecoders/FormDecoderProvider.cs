
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpFramework.FormDecoders
{
    /// <summary>
    /// This provider is used to let us implement any type of form decoding we want without
    /// having to rewrite anything else in the server.
    /// </summary>
    public class FormDecoderProvider
    {
        private readonly IList<IFormDecoder> _decoders = new List<IFormDecoder>();
        private IFormDecoder _defaultDecoder;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType">Should contain boundary and type, as in: multipart/form-data; boundary=---------------------------230051238959</param>
        /// <param name="stream">Stream containing form data.</param>
        /// <param name="encoding">Encoding used when decoding the stream</param>
        /// <returns><see cref="HttpInput.Empty"/> if no parser was found.</returns>
        /// <exception cref="ArgumentException">If stream is null or not readable.</exception>
        /// <exception cref="InvalidDataException">If stream contents cannot be decoded properly.</exception>
        public HttpForm Decode(string contentType, Stream stream, Encoding encoding)
        {
			if (_decoders.Count == 0 && _defaultDecoder == null)
				return HttpForm.EmptyForm;

            if (encoding == null)
                encoding = Encoding.UTF8;
            if (stream == null || !stream.CanRead)
                throw new ArgumentException("Stream is null or not readable.");

            if (string.IsNullOrEmpty(contentType))
            {
                return _defaultDecoder != null
                           ? _defaultDecoder.Decode(stream, contentType, encoding)
                           : HttpForm.EmptyForm;
            }

            //multipart/form-data; boundary=---------------------------230051238959
            foreach (IFormDecoder decoder in _decoders)
            {
                if (decoder.CanParse(contentType))
                    return decoder.Decode(stream, contentType, encoding);
            }

			return HttpForm.EmptyForm;
        }

        /// <summary>
        /// Add a decoder.
        /// </summary>
        /// <param name="decoder"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Add(IFormDecoder decoder)
        {
            if (decoder == null)
                throw new ArgumentNullException("decoder");
            if (_decoders.Contains(decoder))
                return;

            _decoders.Add(decoder);
        }

        /// <summary>
        /// Number of added decoders.
        /// </summary>
        public int Count
        {
            get { return _decoders.Count; }
        }

        /// <summary>
        /// Use with care.
        /// </summary>
        public IList<IFormDecoder> Decoders
        {
            get { return _decoders; }
        }

        /// <summary>
        /// Decoder used for unknown content types.
        /// </summary>
        public IFormDecoder DefaultDecoder
        {
            get { return _defaultDecoder; }
            set { _defaultDecoder = value; }
        }
    }
}
