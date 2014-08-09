<Query Kind="Program" />

void Main()
{
	int binSizeMb = 4476; // This is the (floor of the) total size of a DVD+R reported by CDBurnerXP.	
	string rootFileFolderPath = @"F:\2006 - Polyester Pimpstrap Intergalactic Extravaganza multicam";
	
	IEnumerable<SimpleFileInfo> files = GetFiles(rootFileFolderPath);
	List<Bin> packedBins = PackFiles(files, binSizeMb);
	
	files.Dump();
	packedBins.Dump();
}

// Define other methods and classes here

IEnumerable<SimpleFileInfo> GetFiles(string path)
{
	const int bytesPerMb = 1048576;
	
	return
		(from FileInfo file in (new DirectoryInfo(path)).EnumerateFiles("*", SearchOption.AllDirectories)
		 select new SimpleFileInfo(){
		 	Path = file.FullName,
			SizeMb = (int)Math.Ceiling((decimal)(file.Length / bytesPerMb))
		 })
		 .OrderByDescending (f => f.SizeMb);
}

List<Bin> PackFiles(IEnumerable<SimpleFileInfo> files, int maxBinSizeMb)
{
	List<Bin> bins = new List<Bin>();
	
	foreach (var file in files)
	{
		Bin firstAvailableBin =
			bins.Where(b => b.RemainingSizeMb >= file.SizeMb)
			.FirstOrDefault();
		
		if (firstAvailableBin == null)
		{
			Bin newBin = new Bin(maxBinSizeMb);
			bins.Add(newBin);
			newBin.Files.Add(file);
		}
		else
		{
			firstAvailableBin.Files.Add(file);
		}
	}
	
	return bins;
}

class SimpleFileInfo {
	public string Path { get; set; }
	public int SizeMb { get; set; }
}

class Bin {
	private int _maxSizeMb;
	
	public Bin(int maxSizeMb)
	{
		_maxSizeMb = maxSizeMb;
		this.Files = new List<SimpleFileInfo>();
	}

	public List<SimpleFileInfo> Files { get; set; }
	
	public int RemainingSizeMb
	{
		get
		{
			return 
				_maxSizeMb
				- Files.Sum(f => f.SizeMb);
		}
	}
}

// TODO: Add unit tests!