using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace photoSorter
{
    class Program
    {
        static void Main(string[] args)
        {

            /*
            - Go to directory
            - list files in directory
            - for eaxh file
            -   if image, check exif date
            -    1   if photobatch with date exists add to photobatch
            -    2   if photobatch does not exist, check if another file has the same date +/-1 in the list of photobatch
            -    3   if another file has the date right before or after, add to batch, change start or end date
            -    4   if no other file has the date in the list, create a new entry
            
            - when last file reached:
            -   all images in photobatches with only one image are left in the current directory
            -   all images in photobatches with more than one image has a directory created with pattern YYYY-MM-DD
            */

            string directory = SetWorkingDirectory(args);
            List<PhotoBatch> photoBatches = new List<PhotoBatch>();



            DirectoryInfo di = new DirectoryInfo(directory);
            FileInfo[] files = di.GetFiles();

            foreach (FileInfo file in files)
            {
                DateTime imageDate = new DateTime();
                try
                {
                    imageDate = GetExifDate(file.FullName);
                }
                catch (ImageProcessingException)
                {
                    Console.WriteLine($"{file.FullName} is Probably not an image file");
                }
                if (!(imageDate == DateTime.MinValue))
                {
                    // Check for condition 1 as of above
                    int index = photoBatches.FindIndex(c => (imageDate >= c.StartDate.AddDays(-1) && imageDate <= c.EndDate.AddDays(1)));
                    if (index >= 0)
                        photoBatches[index].AddFile(file, imageDate);
                    else
                        photoBatches.Add(new PhotoBatch(file, imageDate));

                }
            }

            foreach (PhotoBatch p in photoBatches.Where(pb => pb.Files.Count > 1))
            {
                DirectoryInfo newDir = System.IO.Directory.CreateDirectory(p.BatchName);
                p.Files.ForEach(f => f.MoveTo($"{newDir.FullName}\\{f.Name}"));
            }
        }



        private static string SetWorkingDirectory(string[] args)
        {
            string directory = System.IO.Directory.GetCurrentDirectory();
            if (args.Length > 0)
            {
                directory = args[0];
                try
                {
                    //Set the current directory.
                    System.IO.Directory.SetCurrentDirectory(directory);
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine("The specified directory does not exist. {0}", e);
                }
            }

            return directory;
        }

        private static DateTime GetExifDate(string file)
        {
            DateTime dateValue;
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);
            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            string dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
            if (!String.IsNullOrEmpty(dateTime))
            {
                string date = dateTime.Substring(0, 10);
                date = date.Replace(':', '/');
                if (DateTime.TryParse(date, out dateValue))
                {
                    return dateValue;
                }
            }
            return DateTime.MinValue;

        }

        private static void PrintAllExifData(string imagePath)
        {
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(imagePath);
            foreach (var directory in directories)
                foreach (var tag in directory.Tags)
                    Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
        }
    }
}
