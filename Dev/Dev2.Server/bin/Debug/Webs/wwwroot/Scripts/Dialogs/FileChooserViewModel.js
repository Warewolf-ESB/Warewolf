// Make this available to chrome debugger
//@ sourceURL=FileChooserViewModel.js  

function FileChooserViewModel(environment) {
    var self = this;

    var $fileTree = $("#fileTree");
    var $chooserOkButton = $("#okButton");

    self.currentEnvironment = ko.observable(environment);

    self.filePaths = ko.observableArray();
        
    self.title = ko.observable("Choose File(s)");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });

    self.isFormValid = ko.computed(function () {
        return self.filePaths().length > 0;
    });
    
    self.enableSaveButton = function (enabled) {
        $chooserOkButton.button("option", "disabled", !enabled);
    };

    self.cancel = function () {
        studio.cancel();
        return true;
    }; 

    self.save = function () {
        var data = { filePaths: self.filePaths() };
        var jsonData = ko.toJSON(data);
        //This is done because C# escapes double qoutes
        jsonData = jsonData.replace(/"/g, "'");
        studio.saveAndClose(jsonData);
    };
    
	// set root URL
	var baseUrl = utils.parseBaseURL(window.location + "");

	self.processMatchingPaths = function (childNode, action) {
        var files = self.filePaths();
        for (var i = 0; i < files.length; i++) {
            var current = files[i];
            //use childNode.getLevel to find the file location part
            if (childNode.data.title == current.split("\\")[childNode.getLevel() - 1]) {
                action();
            }
        }
	};

    self.treePathLoaded = true;
    self.initializeDynatree = function() {
        $.ajax({
            type: 'POST',
            url: baseUrl + "/Services/FindDriveService",
            data: '',
            success: function(fullResult) {
                $fileTree.dynatree({
                    checkbox: true,
                    selectMode: 3,
                    children: fullResult,
                    onDblClick: function (node, event) {
                        node.toggleSelect();
                    },
                    onKeydown: function (node, event) {
                        if (event.which == 32) {
                            node.toggleSelect();
                            return false;
                        }
                        return true;
                    },
                    onSelect: function (isSelected, node) {
                        if (node.data.isFolder) {
                            return;
                        }
                        if (isSelected) {
                            self.filePaths.push(self.resolvePath(node));
                        } else {
                            self.filePaths.remove(self.resolvePath(node));
                        }                    
                    },
                    onCreate: function(node, nodeSpan) {
                        if (!node.data.isFolder) {
                            self.removeExpander(node); //remove expander icon
                        }
                    },
                    onLazyRead: function(node) {
                        if (node.data.isFolder) {
                            $.post(baseUrl + "/Services/FindDirectoryService?DirectoryPath=" + self.resolvePath(node), "", function(lazyResult) {
                                node.setLazyNodeStatus(DTNodeStatus_Ok);
                                if (lazyResult.ErrorMessage == null) {
                                    node.addChild(lazyResult);
                                } else {
                                    console.log(lazyResult.ErrorMessage);
                                }
                            }).success(function() {
                                var files = self.filePaths();
                                //highjack lazyload to load in file location into the tree (this avoids using loadKeyPath)
                                if (!self.treePathLoaded && files.length > 0) {
                                    node.visit(function (childNode) {
                                        self.processMatchingPaths(childNode, function() {
                                            childNode.expand(true); //trigger recursive call
                                            if (!childNode.data.isFolder) {
                                                $fileTree.animate({
                                                    // animate the scrolling to the node
                                                    scrollTop: ($(childNode.li).offset().top - 150) - $fileTree.offset().top + $fileTree.scrollTop()
                                                }, 'fast');
                                                $(".dynatree-title", childNode.li).addClass("dynatree-selectedtitle");
                                            }
                                        });                                        
                                    });
                                }
                                if (node.getChildren() == null) {
                                    self.removeExpander(node); //remove expander icon
                                }
                            });
                        } else {
                            node.setLazyNodeStatus(DTNodeStatus_Ok);
                            self.treePathLoaded = true; //don't allow lazy load to be hijacked to load assembly paths anymore
                        }
                    }
                });
            },
            dataType: 'json',
            async: false
        });
    };
    
    self.removeExpander = function (node) {
        var $li = $(node.li);
        var $expander = $li.find(".dynatree-expander");
        $expander.remove();

        $li.css("margin-left", "16px");
        //node.render();
    };
    
    self.load = function () {
        self.title("Choose File(s)");
        self.treePathLoaded = false;
        self.loadTreePath();
    };


    //load a path into the tree
    self.loadTreePath = function () {
        self.treePathLoaded = false;
        self.initializeDynatree();
        var treeRoot = $fileTree.dynatree("getRoot");
        treeRoot.visit(function (childNode) {
            self.processMatchingPaths(childNode, function() {
                childNode.expand(true);//triggers lazyload which recursively loads all of assembly location into the dynatree (this is only on load, treePathLoaded is the flag to ensure this)
            });
        });
    };

    self.resolvePath = function (node) {
        var fullNodePath = "";
        var nodeIterator = node;

        for (var i = 0; i < node.getLevel() ; i++) {
            fullNodePath = nodeIterator.data.title + "\\" + fullNodePath;
            nodeIterator = nodeIterator.getParent();
        }
		
        if (!node.data.isFolder) {
            fullNodePath = fullNodePath.substr(0, fullNodePath.lastIndexOf('\\'));
        }
        return fullNodePath;
    };

    self.getFileFromPath = function (path) {
        return path.substr(path.lastIndexOf("\\") + 1, path.length - path.lastIndexOf("\\"));
    };
  
    self.load();
};

FileChooserViewModel.create = function (fileChooserContainerID) {
    // apply jquery-ui themes
    $("button").button();

    var fileChooserViewModel = new FileChooserViewModel(utils.decodeFullStops(getParameterByName("envir")));
    ko.applyBindings(fileChooserViewModel, document.getElementById(fileChooserContainerID));
    return fileChooserViewModel;
};