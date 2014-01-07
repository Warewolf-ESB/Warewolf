using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Dev2.Studio.Core.Models
{
    [Export(typeof(ITagCloudItemModel))]
    public class TagCloudItemModel : ITagCloudItemModel
    {
        readonly List<TagModel> _tags;

        public TagCloudItemModel(List<TagModel> tags)
        {
            _tags = tags;
        }

        public string Tag { get; set; }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if(!string.IsNullOrEmpty(Tag))
                {
                    _tags.Where(tag => tag.Tag == Tag).ToList().ForEach(t => t.IsSelected = value);
                }
                _isSelected = value;
            }
        }

        public string TagLabel
        {
            get
            {
                if(!string.IsNullOrEmpty(Tag))
                {
                    return string.Format("{0} ({1})", Tag, Count);
                }
                return null;
            }

        }
        public double Size { get; set; }
        public int Count { get; set; }




    }
}
