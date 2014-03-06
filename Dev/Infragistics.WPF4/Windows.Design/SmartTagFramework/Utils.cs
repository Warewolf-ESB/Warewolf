using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// A static class containing common static methods. 
    /// </summary>
    public static class Utils
    {
        #region Methods

        #region Public Methods

        #region PropertyHelper

        /// <summary>
        /// Helper method to retrieve control properties.     
        /// </summary>
        /// <param name="controlType">Control type</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>PropertyDescriptor</returns>
        public static PropertyDescriptor PropertyHelper(Type controlType, string propertyName)
        {
            PropertyDescriptor property;
            property = TypeDescriptor.GetProperties(controlType)[propertyName];
            if (null == property)
            {
                throw new ArgumentException("Property not found!", propertyName);
            }
            else
            {
                return property;
            }
        }

        #endregion //PropertyHelper

        #region MethodHelper

        /// <summary>
        /// Creates an instace of MethodInfo for the specified Type and method name.
        /// </summary>
        /// <param name="currentType">Control type</param>
        /// <param name="methodName">Method name</param>
        /// <returns>MethodInfo</returns>
        public static MethodInfo MethodHelper(Type currentType, string methodName)
        {
            MethodInfo methodInfo = currentType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (null == methodInfo)
            {
                throw new ArgumentException("Method not found!", methodName);
            }
            else
            {
                return methodInfo;
            }
        }

        #endregion //MethodHelper

        #region HexStringToColor

        /// <summary>
        /// Convert a hex string to a .NET Color object.
        /// </summary>
        /// <param name="hexColor">a hex string: "FFFFFFFF", "#00000000"</param>
        public static Color HexStringToColor(string hexColor)
        {
            string hc = ExtractHexDigits(hexColor);
            if (hc.Length != 8)
            {
                throw new ArgumentException("The Color is not exactly 8 digits.");
            }

            string a = hc.Substring(0, 2);
            string r = hc.Substring(2, 2);
            string g = hc.Substring(4, 2);
            string b = hc.Substring(6, 2);

            Color color = new Color();
            try
            {
                Byte ai = Byte.Parse(a, System.Globalization.NumberStyles.HexNumber);
                Byte ri = Byte.Parse(r, System.Globalization.NumberStyles.HexNumber);
                Byte gi = Byte.Parse(g, System.Globalization.NumberStyles.HexNumber);
                Byte bi = Byte.Parse(b, System.Globalization.NumberStyles.HexNumber);
                color = Color.FromArgb(ai, ri, gi, bi);
            }
            catch
            {
                throw new ArgumentException("Color Conversion failed.");
            }
            return color;
        }

        #endregion //HexStringToColor

        #region HexStringToBrush

        /// <summary>
        /// Convert a hex string to a .NET Brush object.
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
        public static Brush HexStringToBrush(string hexColor)
        {
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = HexStringToColor(hexColor);
            return brush;
        }

        #endregion //HexStringToBrush

        #region StringToBrush

        /// <summary>
        /// Converts predefned Color name to .NET Brush object
        /// </summary>
        /// <param name="colorString"></param>
        /// <returns></returns>
        public static Brush StringToBrush(string colorString)
        {
            SolidColorBrush brush = new SolidColorBrush();

            if (colorString.Substring(0, 1).Equals("#"))
            {
                brush.Color = HexStringToColor(colorString);
            }
            else
            {
                foreach (NamedColor namedColor in EditorDataProvider.DefaultColorNames)
                {
                    if (namedColor.Name.Equals(colorString))
                    {
                        brush.Color = namedColor.Color;
                        break;
                    }
                }
            }

            return brush;
        }

        #endregion //StringToBrush

        #region ExtractHexDigits

        /// <summary>
        /// Extract only the hex digits from a string.
        /// </summary>
        public static string ExtractHexDigits(string input)
        {
            // remove any characters that are not digits (like #)
            Regex isHexDigit = new Regex("[abcdefABCDEF\\d]+", RegexOptions.Compiled);
            string newnum = "";
            foreach (char c in input)
            {
                if (isHexDigit.IsMatch(c.ToString()))
                {
                    newnum += c.ToString();
                }
            }
            return newnum;
        }

        #endregion //ExtractHexDigits

        #region FindResourceType

        /// <summary>
        /// Returns a ResourceType object of the resource
        /// </summary>
        /// <param name="uri">The Uri of the resource</param>
        /// <param name="resourceTypes">List of requred resource types</param>
        /// <returns>ResourceType object</returns>
        public static ResourceType FindResourceType(Uri uri, List<ResourceType> resourceTypes)
        {
            if (string.IsNullOrEmpty(uri.OriginalString))
            {
                return null;
            }

            int position = uri.OriginalString.LastIndexOf(".");
            if (position < 0)
            {
                return null;
            }

            string extension = uri.OriginalString.Substring(position);

            foreach (ResourceType resourceType in resourceTypes)
            {
                if (resourceType.Extension.Equals(extension))
                {
                    return resourceType;
                }
            }

            return null;
        }

        #endregion //FindResourceType

        #region GetImageTypes

        /// <summary>
        /// Creates a list of all image types
        /// </summary>
        /// <returns>List of ResourceType objects</returns>
        public static List<ResourceType> GetImageTypes()
        {
            List<ResourceType> resourceTypes = new List<ResourceType>();

            //resourceTypes.Add(new ResourceType(".001", "Fax File"));
            //resourceTypes.Add(new ResourceType(".2bp", "Pocket PC Bitmap Image File"));
            //resourceTypes.Add(new ResourceType(".2d", "VersaCAD 2D Drawing"));
            //resourceTypes.Add(new ResourceType(".3d", "Stereo CAD-3D Image File"));
            //resourceTypes.Add(new ResourceType(".3d2", "Stereo CAD-3D 2.0 Image File"));
            //resourceTypes.Add(new ResourceType(".3d4", "Stereo CAD-3D 2.0 Image File"));
            //resourceTypes.Add(new ResourceType(".3da", "3D Assembly"));
            //resourceTypes.Add(new ResourceType(".3df", "3D Format"));
            //resourceTypes.Add(new ResourceType(".3dl", "LightConverse 3D Model File"));
            //resourceTypes.Add(new ResourceType(".3dm", "Rhino 3D Model"));
            //resourceTypes.Add(new ResourceType(".3dmf", "QuickDraw 3D Metafile"));
            //resourceTypes.Add(new ResourceType(".3ds", "3D Studio Scene"));
            //resourceTypes.Add(new ResourceType(".3dv", "3D VRML World"));
            //resourceTypes.Add(new ResourceType(".3dx", "Rhino 3D Model File"));
            //resourceTypes.Add(new ResourceType(".8pbs", "Adobe Photoshop Macintosh File"));
            //resourceTypes.Add(new ResourceType(".ac5", "ArtCut 5 Document"));
            //resourceTypes.Add(new ResourceType(".ac6", "ArtCut 6 Document"));
            //resourceTypes.Add(new ResourceType(".acr", "American College of Radiology Format"));
            //resourceTypes.Add(new ResourceType(".act", "Genesis3D Actor File"));
            //resourceTypes.Add(new ResourceType(".adc", "Scanstudio 16 Color Image"));
            //resourceTypes.Add(new ResourceType(".adi", "AutoCAD Device-Independent Binary Plotter File"));
            //resourceTypes.Add(new ResourceType(".afp", "Advanced Function Presentation File"));
            //resourceTypes.Add(new ResourceType(".agp", "ArtGem Project File"));
            //resourceTypes.Add(new ResourceType(".ai", "Adobe Illustrator File"));
            //resourceTypes.Add(new ResourceType(".ais", "ACDSee Image Sequence"));
            //resourceTypes.Add(new ResourceType(".amu", "Sony Photo Album"));
            //resourceTypes.Add(new ResourceType(".anm", "3D Animation File"));
            //resourceTypes.Add(new ResourceType(".apng", "Animated Portable Network Graphic"));
            //resourceTypes.Add(new ResourceType(".ard", "ArtiosCAD Workspace File"));
            //resourceTypes.Add(new ResourceType(".arr", "Amber Graphic File"));
            //resourceTypes.Add(new ResourceType(".art", "AOL Compressed Image File"));
            //resourceTypes.Add(new ResourceType(".art", "Art Document"));
            //resourceTypes.Add(new ResourceType(".asat", "Assemble SAT 3D Model File"));
            //resourceTypes.Add(new ResourceType(".awd", "FaxView Document"));
            //resourceTypes.Add(new ResourceType(".bip", "Character Studio Biped File"));
            //resourceTypes.Add(new ResourceType(".biz", "Broderbund Business Card File"));
            //resourceTypes.Add(new ResourceType(".blend", "Blender 3D Data File"));
            //resourceTypes.Add(new ResourceType(".blkrt", "Block Artist Image File"));
            //resourceTypes.Add(new ResourceType(".blz", "Compressed Bitmap Graphic"));
            //resourceTypes.Add(new ResourceType(".bmc", "Embroidery Image File"));
            //resourceTypes.Add(new ResourceType(".bmc", "Bitmap Cache File"));
            //resourceTypes.Add(new ResourceType(".bmf", "Binary Material File"));
            //resourceTypes.Add(new ResourceType(".bmf", "FloorPlan File"));
            resourceTypes.Add(new ResourceType(".bmp", "Bitmap Image File"));
            //resourceTypes.Add(new ResourceType(".br3", "Bryce 3 Scene File"));
            //resourceTypes.Add(new ResourceType(".br4", "Bryce 4 Scene File"));
            //resourceTypes.Add(new ResourceType(".br5", "Bryce 3D Scene File"));
            //resourceTypes.Add(new ResourceType(".bro", "CreataCard Brochure Project"));
            //resourceTypes.Add(new ResourceType(".bro", "Broadleaf Tree Model"));
            //resourceTypes.Add(new ResourceType(".btf", "NationsBank Check Images"));
            //resourceTypes.Add(new ResourceType(".btif", "NationsBank Check Images"));
            //resourceTypes.Add(new ResourceType(".bvh", "Biovision Hierarchy Animation File"));
            //resourceTypes.Add(new ResourceType(".c4", "JEDMICS Image File"));
            //resourceTypes.Add(new ResourceType(".c4d", "Cinema 4D Model File"));
            //resourceTypes.Add(new ResourceType(".cag", "Clip Art Gallery"));
            //resourceTypes.Add(new ResourceType(".cal", "Calendar File"));
            //resourceTypes.Add(new ResourceType(".cal", "CALS Raster Graphic"));
            //resourceTypes.Add(new ResourceType(".cals", "CALS Raster Graphic File"));
            //resourceTypes.Add(new ResourceType(".cam", "Casio Digital Camera Picture"));
            //resourceTypes.Add(new ResourceType(".catpart", "CATIA V5 Part Document"));
            //resourceTypes.Add(new ResourceType(".cd2", "Click'N Design 3d File"));
            //resourceTypes.Add(new ResourceType(".cdr", "CorelDRAW Image File"));
            //resourceTypes.Add(new ResourceType(".cdt", "CorelDraw Template"));
            //resourceTypes.Add(new ResourceType(".ce", "Computer Eyes Image"));
            //resourceTypes.Add(new ResourceType(".cel", "MicroStation Cell Library"));
            //resourceTypes.Add(new ResourceType(".cgm", "Computer Graphics Metafile"));
            //resourceTypes.Add(new ResourceType(".cil", "ClipArt Gallery Packaged File"));
            //resourceTypes.Add(new ResourceType(".cin", "Kodak Cineon Bitmap File"));
            //resourceTypes.Add(new ResourceType(".cit", "Intergraph Bitmap Image File"));
            //resourceTypes.Add(new ResourceType(".cld", "Canon CD Label Template"));
            //resourceTypes.Add(new ResourceType(".cm2", "Poser Camera Set File"));
            //resourceTypes.Add(new ResourceType(".cmp", "Solid Edge Wire Harness File"));
            //resourceTypes.Add(new ResourceType(".cmx", "Corel Metafile Exchange Image File"));
            //resourceTypes.Add(new ResourceType(".cmz", "Compressed Poser Camera Set File"));
            //resourceTypes.Add(new ResourceType(".cnv", "Canvas 6-8 Drawing File"));
            //resourceTypes.Add(new ResourceType(".comicdoc", "Comic Life Document"));
            //resourceTypes.Add(new ResourceType(".cpc", "CPC Compressed Image File"));
            //resourceTypes.Add(new ResourceType(".cph", "Corel Print House File"));
            //resourceTypes.Add(new ResourceType(".cps", "Corel Photo House File"));
            //resourceTypes.Add(new ResourceType(".cpt", "Corel Photo-Paint Document"));
            //resourceTypes.Add(new ResourceType(".cr2", "Canon Raw Image File"));
            //resourceTypes.Add(new ResourceType(".cr2", "Poser Character Rigging File"));
            //resourceTypes.Add(new ResourceType(".crw", "Canon Raw CIFF Image File"));
            //resourceTypes.Add(new ResourceType(".crz", "Compressed Poser Character Rigging File"));
            //resourceTypes.Add(new ResourceType(".csd", "Compact Shared Document"));
            //resourceTypes.Add(new ResourceType(".csf", "Content Secure Format"));
            //resourceTypes.Add(new ResourceType(".csm", "Character Studio Marker File"));
            //resourceTypes.Add(new ResourceType(".cut", "Dr. Halo Bitmap Image File"));
            //resourceTypes.Add(new ResourceType(".cv5", "Canvas 5 Drawing File"));
            //resourceTypes.Add(new ResourceType(".cvg", "Calamus Vector Graphic File"));
            //resourceTypes.Add(new ResourceType(".cvi", "Canvas Image Format"));
            //resourceTypes.Add(new ResourceType(".cvi", "CassiniVision Map Image File"));
            //resourceTypes.Add(new ResourceType(".cvs", "Canvas 4 Drawing File"));
            //resourceTypes.Add(new ResourceType(".cvx", "Canvas 9 Image File"));
            //resourceTypes.Add(new ResourceType(".dae", "Digital Asset Exchange File"));
            //resourceTypes.Add(new ResourceType(".dc", "DesignCAD Design File"));
            //resourceTypes.Add(new ResourceType(".dcd", "DesignCAD Drawing"));
            //resourceTypes.Add(new ResourceType(".dcm", "DICOM Image File"));
            //resourceTypes.Add(new ResourceType(".dcr", "Kodak RAW Image File"));
            //resourceTypes.Add(new ResourceType(".dcs", "Desktop Color Separation File"));
            //resourceTypes.Add(new ResourceType(".dcx", "FAXserve Fax Document"));
            //resourceTypes.Add(new ResourceType(".ddb", "Device Dependent Bitmap"));
            //resourceTypes.Add(new ResourceType(".ddrw", "ClarisDraw Drawing"));
            //resourceTypes.Add(new ResourceType(".dds", "DirectDraw Surface"));
            //resourceTypes.Add(new ResourceType(".des", "Pro/DESKTOP CAD File"));
            //resourceTypes.Add(new ResourceType(".des", "Corel Designer File"));
            //resourceTypes.Add(new ResourceType(".dff", "RenderWare Model File"));
            //resourceTypes.Add(new ResourceType(".dgn", "MicroStation Drawing File"));
            //resourceTypes.Add(new ResourceType(".dib", "Device Independent Bitmap File"));
            //resourceTypes.Add(new ResourceType(".djvu", "DjVu Image"));
            //resourceTypes.Add(new ResourceType(".dng", "Digital Negative Image File"));
            //resourceTypes.Add(new ResourceType(".dpd", "Ovation Pro File"));
            //resourceTypes.Add(new ResourceType(".dpp", "DrawPlus Drawing File"));
            //resourceTypes.Add(new ResourceType(".dpr", "Digital InterPlot File"));
            //resourceTypes.Add(new ResourceType(".dpx", "Digital Picture Exchange File"));
            //resourceTypes.Add(new ResourceType(".drw", "Drawing File"));
            //resourceTypes.Add(new ResourceType(".drw", "DESIGNER Drawing"));
            //resourceTypes.Add(new ResourceType(".dtp", "Publish-iT Document"));
            //resourceTypes.Add(new ResourceType(".dvl", "Virtual Library File"));
            //resourceTypes.Add(new ResourceType(".dwf", "Design Web Format File"));
            //resourceTypes.Add(new ResourceType(".dwg", "AutoCAD Drawing Database File"));
            //resourceTypes.Add(new ResourceType(".dxb", "Drawing Exchange Binary"));
            //resourceTypes.Add(new ResourceType(".dxf", "Drawing Exchange Format File"));
            //resourceTypes.Add(new ResourceType(".emf", "Enhanced Windows Metafile"));
            //resourceTypes.Add(new ResourceType(".emz", "Windows Compressed Enhanced Metafile"));
            //resourceTypes.Add(new ResourceType(".enc", "Copysafe Protected PDF File"));
            //resourceTypes.Add(new ResourceType(".eps", "Encapsulated PostScript File"));
            //resourceTypes.Add(new ResourceType(".exif", "Exchangeable Image Information File"));
            //resourceTypes.Add(new ResourceType(".fac", "FACE Graphic"));
            //resourceTypes.Add(new ResourceType(".face", "FACE Graphic"));
            //resourceTypes.Add(new ResourceType(".fal", "Bitmap Graphic Header Information"));
            //resourceTypes.Add(new ResourceType(".fax", "Fax Document"));
            //resourceTypes.Add(new ResourceType(".fbm", "Fuzzy Bitmap"));
            //resourceTypes.Add(new ResourceType(".fbx", "Autodesk FBX Interchange File"));
            //resourceTypes.Add(new ResourceType(".fc2", "Poser Face Pose File"));
            //resourceTypes.Add(new ResourceType(".fcd", "FastCAD DOS Drawing"));
            //resourceTypes.Add(new ResourceType(".fcw", "FastCAD Windows Drawing"));
            //resourceTypes.Add(new ResourceType(".fcz", "Compressed Poser Face Pose File"));
            //resourceTypes.Add(new ResourceType(".fd2", "PictureMate Borders File"));
            //resourceTypes.Add(new ResourceType(".fh9", "FreeHand 9 Drawing File"));
            //resourceTypes.Add(new ResourceType(".fhd", "FreeHand Drawing File"));
            //resourceTypes.Add(new ResourceType(".fig", "Xfig Drawing"));
            //resourceTypes.Add(new ResourceType(".fil", "Symbian Application Logo File"));
            //resourceTypes.Add(new ResourceType(".fits", "Flexible Image Transport System"));
            //resourceTypes.Add(new ResourceType(".flx", "FelixCAD Drawing"));
            //resourceTypes.Add(new ResourceType(".fm", "FrameMaker Document"));
            //resourceTypes.Add(new ResourceType(".fp3", "FloorPlan 3D Design File"));
            //resourceTypes.Add(new ResourceType(".fpx", "FlashPix Bitmap Image File"));
            //resourceTypes.Add(new ResourceType(".fs", "FlexiSIGN Document"));
            //resourceTypes.Add(new ResourceType(".g", "BRL-CAD Geometry File"));
            //resourceTypes.Add(new ResourceType(".gds", "Graphic Design System"));
            //resourceTypes.Add(new ResourceType(".gem", "Ventura Publisher Document"));
            //resourceTypes.Add(new ResourceType(".gem", "GEM Metafile"));
            //resourceTypes.Add(new ResourceType(".geo", "VRML Geography File"));
            //resourceTypes.Add(new ResourceType(".gfb", "GIFBlast Image"));
            resourceTypes.Add(new ResourceType(".gif", "Graphical Interchange Format File"));
            //resourceTypes.Add(new ResourceType(".gks", "Graphics Kernel System"));
            //resourceTypes.Add(new ResourceType(".gmf", "Geology Multi-File"));
            //resourceTypes.Add(new ResourceType(".graffle", "OmniGraffle Drawing"));
            //resourceTypes.Add(new ResourceType(".gry", "Grayscale Image"));
            //resourceTypes.Add(new ResourceType(".gsd", "Graphtec Vector Graphics File"));
            //resourceTypes.Add(new ResourceType(".gsm", "Graphic Description Language File"));
            //resourceTypes.Add(new ResourceType(".hcx", "ChartXL Chart"));
            //resourceTypes.Add(new ResourceType(".hd2", "Poser Hand Pose File"));
            //resourceTypes.Add(new ResourceType(".hdp", "HD Photo File"));
            //resourceTypes.Add(new ResourceType(".hdr", "High Dynamic Range Image File"));
            //resourceTypes.Add(new ResourceType(".hdz", "Compressed Poser Hand Pose File"));
            //resourceTypes.Add(new ResourceType(".hf", "HF Image"));
            //resourceTypes.Add(new ResourceType(".hip", "Houdini Project File"));
            //resourceTypes.Add(new ResourceType(".hipnc", "Houdini Apprentice File"));
            //resourceTypes.Add(new ResourceType(".hmk", "Hallmark Card Studio File"));
            //resourceTypes.Add(new ResourceType(".hpgl", "HP Graphics Language Plotter File"));
            //resourceTypes.Add(new ResourceType(".hpi", "Hemera Photo Objects Image File"));
            //resourceTypes.Add(new ResourceType(".hpl", "HP-GL Plotter File"));
            //resourceTypes.Add(new ResourceType(".hr", "TRS-80 Graphic"));
            //resourceTypes.Add(new ResourceType(".hr2", "Poser Hair File"));
            //resourceTypes.Add(new ResourceType(".hrf", "Hitachi Raster Format"));
            //resourceTypes.Add(new ResourceType(".hrz", "Compressed Poser Hair File"));
            //resourceTypes.Add(new ResourceType(".iam", "Inventor Assembly File"));
            //resourceTypes.Add(new ResourceType(".ic1", "Low Resolution Imagic Graphic"));
            //resourceTypes.Add(new ResourceType(".ic2", "Medium Resolution Imagic Graphic"));
            //resourceTypes.Add(new ResourceType(".ic3", "High Resolution Imagic Graphic"));
            //resourceTypes.Add(new ResourceType(".ica", "Image Object Content Architecture (IOCA) File"));
            //resourceTypes.Add(new ResourceType(".icb", "Targa ICB Bitmap Image"));
            //resourceTypes.Add(new ResourceType(".icn", "Windows Icon File"));
            //resourceTypes.Add(new ResourceType(".icon", "Icon Image File"));
            //resourceTypes.Add(new ResourceType(".ico", "Icon File"));  //this is a system file
            //resourceTypes.Add(new ResourceType(".ics", "IronCAD 3D Drawing File"));
            //resourceTypes.Add(new ResourceType(".idw", "Inventor Drawing"));
            //resourceTypes.Add(new ResourceType(".iff", "Amiga IFF Graphic"));
            //resourceTypes.Add(new ResourceType(".iges", "IGES File"));
            //resourceTypes.Add(new ResourceType(".igs", "IGES Drawing File"));
            //resourceTypes.Add(new ResourceType(".ilbm", "Deluxe Paint Graphic"));
            //resourceTypes.Add(new ResourceType(".imj", "JFIF Bitmap Image"));
            //resourceTypes.Add(new ResourceType(".indd", "Adobe InDesign File"));
            //resourceTypes.Add(new ResourceType(".indt", "InDesign Template"));
            //resourceTypes.Add(new ResourceType(".info", "ZoomBrowser Image Index File"));
            //resourceTypes.Add(new ResourceType(".ink", "Pantone Reference File"));
            //resourceTypes.Add(new ResourceType(".ink", "Pocket PC Handwritten Note"));
            //resourceTypes.Add(new ResourceType(".int", "SGI Integer Image"));
            //resourceTypes.Add(new ResourceType(".inx", "InDesign Interchange File"));
            //resourceTypes.Add(new ResourceType(".ipt", "Inventor Part File"));
            //resourceTypes.Add(new ResourceType(".ithmb", "iPod Photo Thumbnails"));
            //resourceTypes.Add(new ResourceType(".ivr", "Image World"));
            //resourceTypes.Add(new ResourceType(".j", "JPEG Image"));
            //resourceTypes.Add(new ResourceType(".j2c", "JPEG 2000 Code Stream"));
            //resourceTypes.Add(new ResourceType(".j2k", "JPEG 2000 Image"));
            //resourceTypes.Add(new ResourceType(".jas", "Paint Shop Pro Compressed Graphic"));
            //resourceTypes.Add(new ResourceType(".jbf", "Paint Shop Pro Browser Cache"));
            //resourceTypes.Add(new ResourceType(".jbig", "Joint Bi-level Image Group File"));
            //resourceTypes.Add(new ResourceType(".jbr", "Paint Shop Pro Brushes File"));
            //resourceTypes.Add(new ResourceType(".jfi", "JPEG File Interchange"));
            //resourceTypes.Add(new ResourceType(".jfif", "JPEG File Interchange Format"));
            //resourceTypes.Add(new ResourceType(".jif", "JPEG Image File"));
            //resourceTypes.Add(new ResourceType(".jiff", "JPEG Image File Format"));
            //resourceTypes.Add(new ResourceType(".jng", "JPEG Network Graphic"));
            //resourceTypes.Add(new ResourceType(".jp2", "JPEG 2000 Core Image File"));
            //resourceTypes.Add(new ResourceType(".jpc", "JPEG 2000 Code Stream File"));
            //resourceTypes.Add(new ResourceType(".jpe", "JPEG Image"));
            resourceTypes.Add(new ResourceType(".jpeg", "JPEG Image File"));
            //resourceTypes.Add(new ResourceType(".jpf", "JPEG 2000 Image"));
            resourceTypes.Add(new ResourceType(".jpg", "JPEG Image File"));
            //resourceTypes.Add(new ResourceType(".jpw", "World File for JPEG"));
            //resourceTypes.Add(new ResourceType(".jpx", "JPEG 2000 Image File"));
            //resourceTypes.Add(new ResourceType(".jtf", "JPEG Tagged Interchange Format"));
            //resourceTypes.Add(new ResourceType(".kdc", "Kodak Photo-Enhancer File"));
            //resourceTypes.Add(new ResourceType(".kdk", "Kodak Proprietary Decimated TIFF Format"));
            //resourceTypes.Add(new ResourceType(".kfx", "Kofax Image File"));
            //resourceTypes.Add(new ResourceType(".kodak", "Kodak Photo CD File"));
            //resourceTypes.Add(new ResourceType(".kpg", "Kai's Power Goo Graphic"));
            //resourceTypes.Add(new ResourceType(".lab", "WordPerfect Label Definition File"));
            //resourceTypes.Add(new ResourceType(".lbm", "Deluxe Paint Bitmap Image"));
            //resourceTypes.Add(new ResourceType(".lin", "AutoCAD Linetype File"));
            //resourceTypes.Add(new ResourceType(".lt2", "Poser Light Set File"));
            //resourceTypes.Add(new ResourceType(".ltz", "Compressed Poser Light Set File"));
            //resourceTypes.Add(new ResourceType(".lwo", "LightWave 3D Object File"));
            //resourceTypes.Add(new ResourceType(".lws", "LightWave 3D Scene File"));
            //resourceTypes.Add(new ResourceType(".lxf", "LEGO Digital Designer Model File"));
            //resourceTypes.Add(new ResourceType(".ma", "Maya Project File"));
            //resourceTypes.Add(new ResourceType(".mac", "MacPaint Image"));
            //resourceTypes.Add(new ResourceType(".mag", "Access Diagram"));
            //resourceTypes.Add(new ResourceType(".max", "PaperPort Scanned Document"));
            //resourceTypes.Add(new ResourceType(".max", "OmniPage Scanned Document"));
            //resourceTypes.Add(new ResourceType(".max", "3D Studio Max Model File"));
            //resourceTypes.Add(new ResourceType(".mb", "Maya Binary Project File"));
            //resourceTypes.Add(new ResourceType(".mbm", "Multi Bitmap File"));
            //resourceTypes.Add(new ResourceType(".mc5", "Poser 5 Material File"));
            //resourceTypes.Add(new ResourceType(".mc6", "Poser 6 Material File"));
            //resourceTypes.Add(new ResourceType(".mcs", "Mathcad Image"));
            //resourceTypes.Add(new ResourceType(".mcx", "MICRO CADAM-X/6000 Model Data File"));
            //resourceTypes.Add(new ResourceType(".mcz", "Compressed Poser Material File"));
            //resourceTypes.Add(new ResourceType(".mdi", "Microsoft Document Imaging File"));
            //resourceTypes.Add(new ResourceType(".meb", "PRO100 3D Interior Catalog Element"));
            //resourceTypes.Add(new ResourceType(".mesh", "3D Mesh Model"));
            //resourceTypes.Add(new ResourceType(".mgf", "Materials and Geometry Format"));
            //resourceTypes.Add(new ResourceType(".mgs", "MGCSoft Vector Shapes"));
            //resourceTypes.Add(new ResourceType(".mic", "Image Composer File"));
            //resourceTypes.Add(new ResourceType(".mip", "Multiple Image Print File"));
            //resourceTypes.Add(new ResourceType(".mix", "Picture It! Image File"));
            //resourceTypes.Add(new ResourceType(".mix", "PhotoDraw Image File"));
            //resourceTypes.Add(new ResourceType(".mma", "Master Album Maker Digital Photo Album"));
            //resourceTypes.Add(new ResourceType(".mng", "Multiple Network Graphic"));
            //resourceTypes.Add(new ResourceType(".mnm", "Character Studio Marker Name File"));
            //resourceTypes.Add(new ResourceType(".model", "CATIA 3D Model FIle"));
            //resourceTypes.Add(new ResourceType(".mp", "Maya PLE Project File"));
            //resourceTypes.Add(new ResourceType(".mrb", "Multiple Resolution Bitmap"));
            //resourceTypes.Add(new ResourceType(".mrw", "Minolta Raw Image File"));
            //resourceTypes.Add(new ResourceType(".msk", "Paint Shop Pro Mask"));
            //resourceTypes.Add(new ResourceType(".msp", "Microsoft Paint Bitmap Image"));
            //resourceTypes.Add(new ResourceType(".mtx", "MetaStream Scene File"));
            //resourceTypes.Add(new ResourceType(".mtz", "Compressed MetaStream Scene File"));
            //resourceTypes.Add(new ResourceType(".nav", "MSN Application Extension"));
            //resourceTypes.Add(new ResourceType(".ncd", "Nero Cover Designer Document"));
            //resourceTypes.Add(new ResourceType(".nef", "Nikon Raw Image File"));
            //resourceTypes.Add(new ResourceType(".neo", "NeoChrome Bitmap Image"));
            //resourceTypes.Add(new ResourceType(".nff", "Neutral File Format"));
            //resourceTypes.Add(new ResourceType(".nif", "Gamebryo Image"));
            //resourceTypes.Add(new ResourceType(".ntc", "Nikon Capture Custom Curves"));
            //resourceTypes.Add(new ResourceType(".ntf", "MediaFace II CD Label"));
            //resourceTypes.Add(new ResourceType(".obj", "3D Object File"));
            //resourceTypes.Add(new ResourceType(".odc", "OpenDocument Chart"));
            //resourceTypes.Add(new ResourceType(".odg", "OpenDocument Graphic"));
            //resourceTypes.Add(new ResourceType(".odi", "OpenDocument Image"));
            //resourceTypes.Add(new ResourceType(".odif", "Open Document Interchange Format"));
            //resourceTypes.Add(new ResourceType(".off", "Object File Format"));
            //resourceTypes.Add(new ResourceType(".ola", "Online Access File"));
            //resourceTypes.Add(new ResourceType(".omf", "OMF Interchange Image File"));
            //resourceTypes.Add(new ResourceType(".opd", "OmniPage Document"));
            //resourceTypes.Add(new ResourceType(".opf", "FlipAlbum File"));
            //resourceTypes.Add(new ResourceType(".orf", "Olympus RAW File"));
            //resourceTypes.Add(new ResourceType(".ota", "OTA Bitmap"));
            //resourceTypes.Add(new ResourceType(".otb", "Nokia Over The Air Bitmap"));
            //resourceTypes.Add(new ResourceType(".otc", "OpenDocument Chart Template"));
            //resourceTypes.Add(new ResourceType(".otg", "OpenDocument Graphic Template"));
            //resourceTypes.Add(new ResourceType(".oti", "OpenDocument Image Template"));
            //resourceTypes.Add(new ResourceType(".ovw", "Cubase WAVE File Overview"));
            //resourceTypes.Add(new ResourceType(".p21", "Express STEP Data Model File"));
            //resourceTypes.Add(new ResourceType(".p2z", "Compressed Poser Pose File"));
            //resourceTypes.Add(new ResourceType(".p3d", "Peak3D 3D Graphics File"));
            //resourceTypes.Add(new ResourceType(".p65", "PageMaker 6.5 Document"));
            //resourceTypes.Add(new ResourceType(".pac", "STAD Graphic File"));
            //resourceTypes.Add(new ResourceType(".pal", "Dr. Halo Color Palette"));
            //resourceTypes.Add(new ResourceType(".pap", "PanoramaStudio Project File"));
            //resourceTypes.Add(new ResourceType(".pat", "Pattern File"));
            //resourceTypes.Add(new ResourceType(".pat", "3D Patch File"));
            //resourceTypes.Add(new ResourceType(".pbm", "Portable Bitmap Image"));
            //resourceTypes.Add(new ResourceType(".pc1", "Degas Elite Low Res Image File"));
            //resourceTypes.Add(new ResourceType(".pc2", "Degas Elite Medium Res Image File"));
            //resourceTypes.Add(new ResourceType(".pc3", "Degas Elite High Res Image File"));
            //resourceTypes.Add(new ResourceType(".pc6", "PowerCADD 6 Drawing File"));
            //resourceTypes.Add(new ResourceType(".pc7", "PowerCADD 7 Drawing File"));
            //resourceTypes.Add(new ResourceType(".pcd", "Kodak Photo CD Image File"));
            //resourceTypes.Add(new ResourceType(".pct", "Picture File"));
            //resourceTypes.Add(new ResourceType(".pcx", "Paintbrush Bitmap Image File"));
            //resourceTypes.Add(new ResourceType(".pd", "FlexiSIGN 5 Plotter Document"));
            //resourceTypes.Add(new ResourceType(".pdd", "Adobe PhotoDeluxe Image"));
            //resourceTypes.Add(new ResourceType(".pdf", "Portable Document Format File"));
            //resourceTypes.Add(new ResourceType(".pdg", "Print Designer GOLD File"));
            //resourceTypes.Add(new ResourceType(".pe4", "Photo Explorer Thumbnail Archive"));
            //resourceTypes.Add(new ResourceType(".pe4", "PhotoImpact Image Archive"));
            //resourceTypes.Add(new ResourceType(".pef", "Pentax Electronic File"));
            //resourceTypes.Add(new ResourceType(".pfr", "Paint Shop Pro Picture Frame"));
            //resourceTypes.Add(new ResourceType(".pgm", "Portable Gray Map Image"));
            //resourceTypes.Add(new ResourceType(".pi1", "Degas Low Resolution Image File"));
            //resourceTypes.Add(new ResourceType(".pi2", "Degas Medium Resolution Image File"));
            //resourceTypes.Add(new ResourceType(".pi3", "Degas High Resolution Image File"));
            //resourceTypes.Add(new ResourceType(".pi4", "DEGAS Image"));
            //resourceTypes.Add(new ResourceType(".pi5", "DEGAS Image"));
            //resourceTypes.Add(new ResourceType(".pi6", "DEGAS Image"));
            //resourceTypes.Add(new ResourceType(".pic", "Generic Picture File"));
            //resourceTypes.Add(new ResourceType(".pic", "QuickTime PICT Image"));
            //resourceTypes.Add(new ResourceType(".pic", "Houdini Raster Image"));
            //resourceTypes.Add(new ResourceType(".picnc", "Houdini 3D Compositing Image"));
            //resourceTypes.Add(new ResourceType(".pict", "Picture File"));
            //resourceTypes.Add(new ResourceType(".pictclipping", "Picture Clipping"));
            //resourceTypes.Add(new ResourceType(".pix", "BRL-CAD Raw Image File"));
            //resourceTypes.Add(new ResourceType(".pl", "Unix Color Plot File"));
            //resourceTypes.Add(new ResourceType(".pl0", "3D Home Architect Foundation Floor Plan"));
            //resourceTypes.Add(new ResourceType(".pl1", "3D Home Architect Floor Plan"));
            //resourceTypes.Add(new ResourceType(".pl2", "3D Home Architect Second Level Floor Plan"));
            //resourceTypes.Add(new ResourceType(".pla", "ArchiCAD Project Archive"));
            //resourceTypes.Add(new ResourceType(".pln", "ArchiCAD Project File"));
            //resourceTypes.Add(new ResourceType(".plt", "AutoCAD Plotter Document"));
            //resourceTypes.Add(new ResourceType(".plt", "HPGL Plot File"));
            //resourceTypes.Add(new ResourceType(".ply", "Polygon Model File"));
            //resourceTypes.Add(new ResourceType(".pm", "Unix XV Graphic File"));
            //resourceTypes.Add(new ResourceType(".pm3", "PageMaker 3 Document"));
            //resourceTypes.Add(new ResourceType(".pm4", "PageMaker 4 Document"));
            //resourceTypes.Add(new ResourceType(".pm5", "PageMaker 5.0 Document"));
            //resourceTypes.Add(new ResourceType(".pm6", "PageMaker 6.0 Document"));
            resourceTypes.Add(new ResourceType(".png", "Portable Network Graphic"));
            //resourceTypes.Add(new ResourceType(".pnt", "MacPaint File"));
            //resourceTypes.Add(new ResourceType(".pov", "POV-Ray Raytracing Format"));
            //resourceTypes.Add(new ResourceType(".pov", "Prolab Object File"));
            //resourceTypes.Add(new ResourceType(".pp2", "Poser Prop File"));
            //resourceTypes.Add(new ResourceType(".ppm", "Portable Pixmap Image File"));
            //resourceTypes.Add(new ResourceType(".ppp", "Page Plus Publication"));
            //resourceTypes.Add(new ResourceType(".ppx", "PagePlus Template File"));
            //resourceTypes.Add(new ResourceType(".ppz", "Compressed Poser Prop File"));
            //resourceTypes.Add(new ResourceType(".prn", "Printable File"));
            //resourceTypes.Add(new ResourceType(".prt", "Solid Edge Part File"));
            //resourceTypes.Add(new ResourceType(".prt", "Unigraphics Part File"));
            //resourceTypes.Add(new ResourceType(".prw", "Artlantis Shader Preview File"));
            //resourceTypes.Add(new ResourceType(".ps", "PostScript File"));
            //resourceTypes.Add(new ResourceType(".psb", "Photoshop Large Document Format"));
            //resourceTypes.Add(new ResourceType(".psd", "Photoshop Document"));
            //resourceTypes.Add(new ResourceType(".psf", "PhotoStudio File"));
            //resourceTypes.Add(new ResourceType(".psg", "Page Segment File"));
            //resourceTypes.Add(new ResourceType(".psid", "PostScript Image Data"));
            //resourceTypes.Add(new ResourceType(".psm", "Solid Edge Sheet Metal File"));
            //resourceTypes.Add(new ResourceType(".psp", "Paint Shop Pro Image File"));
            //resourceTypes.Add(new ResourceType(".pspimage", "Paint Shop Pro Image"));
            //resourceTypes.Add(new ResourceType(".ptx", "Pentax RAW Image File"));
            //resourceTypes.Add(new ResourceType(".ptx", "Paint Shop Pro Texture File"));
            //resourceTypes.Add(new ResourceType(".pwp", "PhotoWorks Image"));
            //resourceTypes.Add(new ResourceType(".pws", "Print Workshop Image"));
            //resourceTypes.Add(new ResourceType(".px", "Pixel Image File"));
            //resourceTypes.Add(new ResourceType(".pxr", "Pixar Image File"));
            //resourceTypes.Add(new ResourceType(".pz2", "Poser Pose File"));
            //resourceTypes.Add(new ResourceType(".pz3", "Poser Scene File"));
            //resourceTypes.Add(new ResourceType(".pzz", "Compressed Poser Scene File"));
            //resourceTypes.Add(new ResourceType(".qif", "QuickTime Image File"));
            //resourceTypes.Add(new ResourceType(".qti", "QuickTime Image File"));
            //resourceTypes.Add(new ResourceType(".qtif", "QuickTime Image File"));
            //resourceTypes.Add(new ResourceType(".qxd", "QuarkXpress Document"));
            //resourceTypes.Add(new ResourceType(".qxp", "QuarkXpress Project File"));
            //resourceTypes.Add(new ResourceType(".qxt", "QuarkXpress Template"));
            //resourceTypes.Add(new ResourceType(".raf", "Fuji RAW Image File"));
            //resourceTypes.Add(new ResourceType(".ras", "Sun Raster Graphic"));
            //resourceTypes.Add(new ResourceType(".raw", "Raw Image Data File"));
            //resourceTypes.Add(new ResourceType(".ray", "Rayshade Image"));
            //resourceTypes.Add(new ResourceType(".rds", "Ray Dream Studio Scene File"));
            //resourceTypes.Add(new ResourceType(".rgb", "RGB Bitmap"));
            //resourceTypes.Add(new ResourceType(".rgb", "Q0 Image File"));
            //resourceTypes.Add(new ResourceType(".rif", "Raster Image File"));
            //resourceTypes.Add(new ResourceType(".rix", "ColorRIX Bitmap Graphic"));
            //resourceTypes.Add(new ResourceType(".rle", "Run Length Encoded Bitmap"));
            //resourceTypes.Add(new ResourceType(".rsr", "Poser Model Preview File"));
            //resourceTypes.Add(new ResourceType(".sar", "Saracen Paint Graphic"));
            //resourceTypes.Add(new ResourceType(".sat", "ACIS SAT Model File"));
            //resourceTypes.Add(new ResourceType(".sbk", "Scrapbook Factory File"));
            //resourceTypes.Add(new ResourceType(".scg", "ColorRIX Bitmap Graphic"));
            //resourceTypes.Add(new ResourceType(".sci", "ColorRIX Bitmap Graphic"));
            //resourceTypes.Add(new ResourceType(".scp", "ColorRIX Bitmap Graphic"));
            //resourceTypes.Add(new ResourceType(".sct", "Scitex Continuous Tone File"));
            //resourceTypes.Add(new ResourceType(".scu", "ColorRIX Bitmap Graphic"));
            //resourceTypes.Add(new ResourceType(".scv", "ScanVec CASmate Sign File"));
            //resourceTypes.Add(new ResourceType(".sda", "OpenOffice.org Draw Document"));
            //resourceTypes.Add(new ResourceType(".sdb", "SAP2000 Model File"));
            //resourceTypes.Add(new ResourceType(".sdm", "Spatial Data Modeling Language"));
            //resourceTypes.Add(new ResourceType(".sdr", "SmartDraw Drawing"));
            //resourceTypes.Add(new ResourceType(".sdt", "SmartDraw Template File"));
            //resourceTypes.Add(new ResourceType(".sff", "Structured Fax Format"));
            //resourceTypes.Add(new ResourceType(".sfw", "Seattle FilmWorks Image"));
            //resourceTypes.Add(new ResourceType(".sgi", "Silicon Graphics Image File"));
            //resourceTypes.Add(new ResourceType(".sh3d", "Sweet Home 3D Design File"));
            //resourceTypes.Add(new ResourceType(".sh3f", "Sweet Home 3D Model Library"));
            //resourceTypes.Add(new ResourceType(".shg", "Segmented Hyper-Graphic"));
            //resourceTypes.Add(new ResourceType(".shp", "Shapes File"));
            //resourceTypes.Add(new ResourceType(".si", "Softimage Image Format"));
            //resourceTypes.Add(new ResourceType(".sid", "MrSID Image"));
            //resourceTypes.Add(new ResourceType(".sig", "Broderbund Sign File"));
            //resourceTypes.Add(new ResourceType(".sim", "Aurora Image"));
            //resourceTypes.Add(new ResourceType(".skp", "SketchUp Document"));
            //resourceTypes.Add(new ResourceType(".sldasm", "SolidWorks Assembly File"));
            //resourceTypes.Add(new ResourceType(".slddrw", "SolidWorks Drawing File"));
            //resourceTypes.Add(new ResourceType(".sldprt", "SolidWorks Part File"));
            //resourceTypes.Add(new ResourceType(".smp", "Xionics SMP Image Format"));
            //resourceTypes.Add(new ResourceType(".snp", "Access Report Snapshot"));
            //resourceTypes.Add(new ResourceType(".sp", "SignPlot Traffic Sign File"));
            //resourceTypes.Add(new ResourceType(".spc", "Spectrum 512 Compressed Image"));
            //resourceTypes.Add(new ResourceType(".spe", "WinSpec CCD Capture File"));
            //resourceTypes.Add(new ResourceType(".spiff", "Still Picture Interchange File Format"));
            //resourceTypes.Add(new ResourceType(".spp", "PhotoPlus Picture File"));
            //resourceTypes.Add(new ResourceType(".spt", "SpeedTree Tree Data File"));
            //resourceTypes.Add(new ResourceType(".spu", "Spectrum 512 Image"));
            //resourceTypes.Add(new ResourceType(".sr", "Sun Raster Image File"));
            //resourceTypes.Add(new ResourceType(".srf", "Sony Raw Image File"));
            //resourceTypes.Add(new ResourceType(".std", "StarOffice Drawing Template"));
            //resourceTypes.Add(new ResourceType(".step", "STEP 3D Model"));
            //resourceTypes.Add(new ResourceType(".stl", "Stereolithography File"));
            //resourceTypes.Add(new ResourceType(".sto", "PRO100 3D Interior Design Project"));
            //resourceTypes.Add(new ResourceType(".stp", "STEP 3D CAD File"));
            //resourceTypes.Add(new ResourceType(".sun", "Sun Raster Graphic"));
            //resourceTypes.Add(new ResourceType(".suniff", "Sun TAAC Graphic"));
            //resourceTypes.Add(new ResourceType(".sup", "Subtitle Bitmap File"));
            //resourceTypes.Add(new ResourceType(".svg", "Scalable Vector Graphics File"));
            //resourceTypes.Add(new ResourceType(".sxd", "StarOffice Drawing"));
            //resourceTypes.Add(new ResourceType(".taac", "Sun TAAC Graphic"));
            //resourceTypes.Add(new ResourceType(".tcd", "Technobox CAD Drawing"));
            //resourceTypes.Add(new ResourceType(".tct", "TurboCAD Drawing Template"));
            //resourceTypes.Add(new ResourceType(".tcw", "TurboCAD Drawing File"));
            //resourceTypes.Add(new ResourceType(".tcx", "TurboCAD 3D Model Text File"));
            //resourceTypes.Add(new ResourceType(".tddd", "3D Data Description"));
            //resourceTypes.Add(new ResourceType(".tex", "Texture File"));
            //resourceTypes.Add(new ResourceType(".tfw", "World File for TIFF"));
            //resourceTypes.Add(new ResourceType(".tga", "Targa Graphic"));
            //resourceTypes.Add(new ResourceType(".thm", "Thumbnail Image File"));
            //resourceTypes.Add(new ResourceType(".thm", "Video Thumbnail File"));
            //resourceTypes.Add(new ResourceType(".thumb", "JAlbum Thumbnail File"));
            //resourceTypes.Add(new ResourceType(".tif", "Tagged Image File"));
            resourceTypes.Add(new ResourceType(".tiff", "Tagged Image File Format"));
            //resourceTypes.Add(new ResourceType(".tjp", "Tiled JPEG File"));
            //resourceTypes.Add(new ResourceType(".tlc", "The Logo Creator File"));
            //resourceTypes.Add(new ResourceType(".tn1", "Tiny Image (Low Resolution)"));
            //resourceTypes.Add(new ResourceType(".tn2", "Tiny Image (Medium Resolution)"));
            //resourceTypes.Add(new ResourceType(".tn3", "Tiny Image (High Resolution)"));
            //resourceTypes.Add(new ResourceType(".tny", "Atari Tiny Image"));
            //resourceTypes.Add(new ResourceType(".trif", "Tiled Raster Interchange Format"));
            //resourceTypes.Add(new ResourceType(".u", "Subsampled Raw YUV Image"));
            //resourceTypes.Add(new ResourceType(".u3d", "Universal 3D File"));
            //resourceTypes.Add(new ResourceType(".ufo", "Ulead File Object"));
            //resourceTypes.Add(new ResourceType(".urt", "Utah Raster Toolkit File"));
            //resourceTypes.Add(new ResourceType(".v", "Subsampled Raw YUV Image"));
            //resourceTypes.Add(new ResourceType(".v3d", "Visual3D.NET Data File"));
            //resourceTypes.Add(new ResourceType(".vda", "Targa Bitmap Image File"));
            //resourceTypes.Add(new ResourceType(".vff", "Sun TAAC Graphic File"));
            //resourceTypes.Add(new ResourceType(".vic", "VICAR Image"));
            //resourceTypes.Add(new ResourceType(".viff", "Visualization Image File Format"));
            //resourceTypes.Add(new ResourceType(".vis", "Visibility Image File"));
            //resourceTypes.Add(new ResourceType(".vna", "JVC JLIP Image"));
            //resourceTypes.Add(new ResourceType(".vrl", "VRML Virtual World"));
            //resourceTypes.Add(new ResourceType(".vsd", "Visio Drawing File"));
            //resourceTypes.Add(new ResourceType(".vss", "Visio Stencil File"));
            //resourceTypes.Add(new ResourceType(".vst", "Targa Bitmap Image"));
            //resourceTypes.Add(new ResourceType(".vst", "Visio Drawing Template"));
            //resourceTypes.Add(new ResourceType(".vtx", "Anim8or 3D Model"));
            //resourceTypes.Add(new ResourceType(".vue", "Vue Scene File"));
            //resourceTypes.Add(new ResourceType(".vvd", "Vivid 3D Scanner Element File"));
            //resourceTypes.Add(new ResourceType(".vwx", "VectorWorks 2008 Design File"));
            //resourceTypes.Add(new ResourceType(".wbmp", "Wireless Bitmap Image File"));
            //resourceTypes.Add(new ResourceType(".wdp", "Windows Media Photo File"));
            //resourceTypes.Add(new ResourceType(".web", "Xara Web Format"));
            //resourceTypes.Add(new ResourceType(".wgs", "Walk-Graph Segment"));
            //resourceTypes.Add(new ResourceType(".wi", "Wavelet Image"));
            //resourceTypes.Add(new ResourceType(".wic", "J Wavelet Image"));
            //resourceTypes.Add(new ResourceType(".wmf", "Windows Metafile"));
            //resourceTypes.Add(new ResourceType(".wmp", "Windows Media Photo File"));
            //resourceTypes.Add(new ResourceType(".wnk", "Wink Screen Capture"));
            //resourceTypes.Add(new ResourceType(".wpg", "WordPerfect Graphic File"));
            //resourceTypes.Add(new ResourceType(".wrl", "VRML World"));
            //resourceTypes.Add(new ResourceType(".wrp", "Geomagic 3D Wrap File"));
            //resourceTypes.Add(new ResourceType(".wrz", "VRML World"));
            //resourceTypes.Add(new ResourceType(".x3d", "Xara3D Project"));
            //resourceTypes.Add(new ResourceType(".xar", "Xara Xtreme Drawing"));
            //resourceTypes.Add(new ResourceType(".xbm", "X11 Bitmap Graphic"));
            //resourceTypes.Add(new ResourceType(".xcf", "GIMP Image"));
            //resourceTypes.Add(new ResourceType(".xdw", "Fuji Xerox DocuWorks File"));
            //resourceTypes.Add(new ResourceType(".xif", "ScanSoft Pagis File"));
            //resourceTypes.Add(new ResourceType(".xof", "Reality Lab 3D Image File"));
            //resourceTypes.Add(new ResourceType(".xpm", "X11 Pixmap Graphic"));
            //resourceTypes.Add(new ResourceType(".xsi", "Softimage XSI 3D Image"));
            //resourceTypes.Add(new ResourceType(".xwd", "X Windows Dump"));
            //resourceTypes.Add(new ResourceType(".xws", "Xara Webstyle Graphic"));
            //resourceTypes.Add(new ResourceType(".y", "Subsampled Raw YUV Image"));
            //resourceTypes.Add(new ResourceType(".yal", "Arts & Letters Clipart Library"));
            //resourceTypes.Add(new ResourceType(".yaodl", "Powerflip 3D Image File"));
            //resourceTypes.Add(new ResourceType(".ydl", "Powerflip YAODL 3D Image File"));
            //resourceTypes.Add(new ResourceType(".yuv", "YUV Encoded Image File"));
            //resourceTypes.Add(new ResourceType(".zgm", "Zenographics Image File"));
            //resourceTypes.Add(new ResourceType(".zif", "Zooming Image Format File"));
            //resourceTypes.Add(new ResourceType(".zno", "Zinio Electronic Magazine File"));
            //resourceTypes.Add(new ResourceType(".zt", "Mental Ray Image Depth File"));

            return resourceTypes;
        }

        #endregion //GetImageTypes

		#region GetDescriptionAttributeForProperty

		/// <summary>
		/// Returns the Description attrinte for the specified property name on the specified owning Type
		/// </summary>
		/// <param name="owningType"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static string GetDescriptionAttributeForProperty(Type owningType, string propertyName)
		{
			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(owningType);
			if (pdc != null)
			{
				DescriptionAttribute da = pdc[propertyName].Attributes[typeof(DescriptionAttribute)] as DescriptionAttribute;
				if (da != null)
					return da.Description;
			}

			return string.Empty;
		}

		#endregion //GetDescriptionAttributeForProperty

        #endregion //Public Methods

        #endregion //Methods
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved