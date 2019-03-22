
## Packer.NET

Hi! Thanks for checking out Packer.NET. So what is Packer.NET you may ask? It's a library that will help you pack lots of files inside a single one. It's perfect for making backups of software files or letting user pack data to be sent over the internet in a single file.

## Downloading
You can either download the latest release from the release page here or add the NuGet package directly to your project. [https://www.nuget.org/packages/Packet.NET/0.1.1.1](https://www.nuget.org/packages/Packet.NET/0.1.1.1).
Or if you prefer you can close this repository and build Packer.NET yourself.

## Usage

After you download Packer.NET and you reference it to your project you can use Packer.NET like this.

Add this like to the top of your project
>using PackerNET;

Then you have to make a Packer Object.
> Packer myPacker = new Packer();

There are 3 events you can subscribe to that are going to help your track the packer's progress and completion

 - Report Backup Progress & Report Restore Progress

>myPacker.ReportBackupProgress
>myPacker.ReportReportProgress

These events will provide you 3 values

 1. **int Current** - The current value of the progress
 2. **int Maximum** - The maximum Value of the progress
 3. **string CurrentFile** - The current file being processed
 
  - Report Progress Completed
 >myPacker.ReportProgressCompleted

This event will fire when any started Backup or Restore progress has been completed

This event will provide you 1 value
1. **long ElapsedMilliseconds** - Milliseconds passed from the start of the Backup/Restore process


Because the Backup/Restore progress is running of separate Threads, in order to use the values on another thread you need to do the following:

Let's say we have button(button1), a progress bar(progressBar1) and a label(label1) on our form and we want to back up a folder when we press the button, report the backup progress and when the backup is completed a to make a message box popup. 

1. First, we make the button's action
```c#
private void button1_Click(object sender, EventArgs e)
{
    Packer myPacker = new PackerNET.Packer(); //Declare the Packer Object
    myPacker .ReportBackupProgress += onReportProgress; //Subscribe to the BackupProgress event
    myPacker .ReportProgressCompleted += onCompletion; //Subscribe to the Completion Progress
    myPacker .BackUp("Path to pack","Path to backup file.extension")
}
            
```
2. Second, we make the events methods
```c#
private void onCompletion(long inn)
    {
        this.Invoke((MethodInvoker)delegate ()
        {
            MessageBox.Show("Backup progress completed");
        });
    }
private void onReportProgress(int current, int max ,string file)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                progressBar1.Maximum =max;
                progressBar1.Value = current;
                Label1.Text = string.Format("{0}/{1} - " + file, current, max);
            });
        }
```

And that's it! Of course, you can skip all the events and just backup.

## The backup/restore file

The backup/restore file does not need to have any special extension. You can use whatever you choose.

## Compression
In the current version of Packer.NET, the file that is generated is not being compressed. But in future updates, a sort of compression is going to be implemented.

## Requirements
Packer.NET require .NET Framework >= 3.5
