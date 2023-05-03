# npsg
needleful's personal site generator

This is an XML-based site generator.  It takes a bunch of XML files from a `src` directory and generates HTML in a `www` folder next to it.

First it searches for files ending with `.template.xml` to create templates. Then it finds any `.page.xml` files and applies the template.

It's not very useful if you don't already know HTML, since it does nothing for you except add `<!DOCTYPE html>` to the top of the file.

The reason I use it is because that's exactly what I wanted.

Something I'd like is nested and recursive templates, which currently don't exist, so variations of the same page have to be copied and pasted.

## Examples

Here's most of `comic.template.xml` used for [my comics](https://needleful.net/comic/needleful/1.html).

```xml
<!-- Comic Formats -->
<template name='comic-needleful'>
	<param name='title' type='text'/>
	<param name='page' type='text,integer'/>
	<param name='source' type='text,file'/>
	<param name='description' type='xml' required='False'/>
	<param name='transcript' type='xml'/>
	<content>
		<html>
			<head>
				<title>{{title}} - Needleful</title>
				<link rel='stylesheet' href='/needleful.css'/>
				<script type='text/javascript' src='/needleful.js'></script>
			</head>
			<body>
				<div id='content'>
					<div id='titlebar'>
						<ul class='navbar'>
							<li><a href='/'>needleful dot net</a> / </li>
							<li><a href='/comic/needleful'>needleful comic</a> /</li>
							<li>page {{page}} </li>
						</ul>
						<!-- more html stuff... -->
					</div>
					<div class='comic-wrapper'>
						<img class='comic' src='{{source}}' alt='Comic: {{title}}'/>
					</div>
					<ul id='comic-buttons'>
						<Match value='{{page == 1}}'>
							<True>
								<li>At the first!</li>
							</True>
							<False>
								<li><a href='/comic/needleful/1.html' class='align-left'>First</a></li>
								<li><a href='/comic/needleful/{{page - 1}}.html' class='align-left'>Previous</a> </li>
							</False>
						</Match>
						
						<li><a href='/comic/needleful/archive' class='align-center'>Archive</a></li>

						<Match f='src-file-exists' x='comic/needleful/{{page + 1}}.page.xml' html-class='inline-button'>
							<True>
								<li><a href='/comic/needleful/{{page + 1}}.html' class='align-right'>Next</a></li>
							</True>
							<False>
								<li>Come back later!</li>
							</False>
						</Match>
					</ul>
					<h1>{{title}}</h1>
					{{description}}
					<h2>Transcript</h2>
					{{transcript}}
					<h2>Latest Comics:</h2>
					<iframe src='latest.html' width='100%' height='400px'/>
				</div>
			</body>
		</html>
	</content>
</template>
```

And here's most of page 1:

```xml
<!-- This comment is exculded from the page -->
<page>
	<!-- This comment is just above the page! -->
	<comic-needleful>
		<title>Confession Time</title>
		<page>1</page>
		<source>img/needleful/confession_time.png</source>
		<!-- Comments outside the parameters are ignored -->
		<description>
			<!--Comments within the parameter are preserved (if the parameter's an XML type)!-->
			<p>talk about oversharing lmao</p>
		</description>
		<transcript>
			<p class='nr'>A boy and a girl sit on the couch.  The girl is under a blanket, snuggling against the boy.</p>
			<!-- the rest of the transcript... -->
		</transcript>
	</comic-needleful>
</page>
```
