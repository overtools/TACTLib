using System.IO;

namespace TACTLib.Core.Product {
    public interface IProductHandler {
        /// <summary>
        /// Open file by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        Stream? OpenFile(object key);
    }
}