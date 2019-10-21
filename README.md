
<table data-table-type="yaml-metadata">
  <thead>
  <tr>
  <th>topic</th>
  <th>languages</th>
  <th>products</th>
  </tr>
  </thead>
  <tbody>
  <tr>
  <td><div>Project</div></td>
  <td><div><table>
  <tbody>
  <tr>
  <td><div>csharp</div></td>
  </tr>
  </tbody>
</table>
</div></td>
  <td><div><table>
  <tbody>
  <tr>
  <td><div>windows</div></td>
  <td><div>windows-uwp</div></td>
  </tr>
  </tbody>
</table>
</div></td>
  </tr>
  </tbody>
</table>

# PhotoBooth

<h2>Notes</h2>

This was a 2 day project and using UWP for the first time. Some things I would improve is implementing the MVVM pattern, during my research I came across the Prism Library (heavyly used by UWP Developers). Prism would allow me to implement the MVVM pattern, use dependency injection where needed, and allow me to do unit testing.

Things To Do Still:

<ul>
	<li>	I was able to read and write pencil drawings to a .jpg file. As well as write the captured image from a webcam. But only able to save 1 or the other. I have tried converting both drawings and image to byte[] and writing that into a single stream, but the results were undesirable. I will continue working on this.
	</li>
	<li>
		Finish unit test methods
	</li>
</ul>




Photo Booth App

Photo Booth App is a Paint like desktop application programmed using C# .NET Framework and UWP (Universal Windows Platform). 
It allows users to draw/paint on a canvas, change the color of their pen from 16 predefined colors. 

Functionalities of this app:
<ul>
<li>Using your mouse, touch or a stylus pen to draw</li>

<li>Selection of various colors</li>

<li>Saving your drawing</li>

<li>Opening saved drawings</li>

<li>Ability to undo 6 times</li>

<li>Using your webcam, internal cam or phone cam to capture an image</li>

<li>Drawing on top of captured image</li>
</ul>
<h2>Requirements</h2>
<ul>
	<li>Windows 10 PC with the latest Windows 10 version (Version 1809 or later)</li>
	<li>Microsoft Visual Studio 2017 or later</li>
	<li>Webcam (for image capture)</li>
</ul>


<h2>Build the sample</h2>

1. Download or Clone the repo. If you download the ZIP, be sure to unzip the entire archive, not just the folder with the sample you want to build.

2. Start Microsoft Visual Studio 2017 or later and select File > Open > Project/Solution. Navigate to the unzipped solutions folder.

3. Press Ctrl+Shift+B, or select Build > Build Solution.

<h2>Running Tests</h2>

<h2>Technologies Used</h2>

UWP .NET Framework 4.6.1
