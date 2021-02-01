// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace PathTools
{
    // Class to hold PATH status, type and path segment
    public class DisplayPath
    {
        public DisplayPath() { }

        public DisplayPath(int seqNumber, string pathStatus, string pathType, string pathDirectory)
        {
            SeqNumber = seqNumber;
            PathStatus = pathStatus;
            PathType = pathType;
            PathDirectory = pathDirectory;
        }
        public static int NumDifferences { get; set; }
        public string PathStatus { get; set; }
        public string PathType { get; set; }
        public string PathDirectory { get; set; }
        public int SeqNumber { get; set; }
        public static int TotalPathLength { get; set; }
        public static int TotalInPath { get; set; }
        public static int TotalAdded { get; set; }
        public static int TotalRemoved { get; set; }
    }
}