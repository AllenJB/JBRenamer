import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

ToolBar {
    Layout.fillWidth: true

    Connections {
        target: files
        function onFileListUpdated() {
            fileCount.text = files.count() + " items";
        }
    }

    Connections {
        target: fileFilters

        function onFiltersUpdated() {
            console.log("filtersUpdated");
            filterStateItemsInFolders.filterEnabled = fileFilters.itemsInFolders;
            filterStateFoldersAsItems.filterEnabled = fileFilters.foldersAsItems;
            filterStateIncludeSubfolders.filterEnabled = fileFilters.includeSubfolders;
            filterStateIncludeHidden.filterEnabled = fileFilters.includeHidden;
            filterStateSkipRootFolders.filterEnabled = fileFilters.skipRootFolders;
        }
    }

    RowLayout {
        FilterButton {
            id: filterStateItemsInFolders
            source: "qrc:/assemblies/JBRenamer/icons/filters_files_in_folders.png"
            tooltip: "Add items within folders"
            filterEnabled: fileFilters.itemsInFolders
        }
        FilterButton {
            id: filterStateFoldersAsItems
            source: "qrc:/assemblies/JBRenamer/icons/filters_folders_as_files.png"
            filterEnabled: fileFilters.foldersAsItems
            tooltip: "Add folders as items"
        }
        FilterButton {
            id: filterStateIncludeSubfolders
            source: "qrc:/assemblies/JBRenamer/icons/filters_subfolders.png"
            filterEnabled: fileFilters.includeSubfolders
            tooltip: "Include subfolders"
        }
        FilterButton {
            id: filterStateIncludeHidden
            source: "qrc:/assemblies/JBRenamer/icons/filters_hidden.png"
            filterEnabled: fileFilters.includeHidden
            tooltip: "Include hidden items"
        }
        FilterButton {
            id: filterStateSkipRootFolders
            source: "qrc:/assemblies/JBRenamer/icons/filters_skip_root_folders.png"
            filterEnabled: fileFilters.skipRootFolders
            tooltip: "Skip root folders when added as items"
        }

        ToolSeparator {
        }

        Label {
            id: fileCount
            text: files.count() + " items"

            horizontalAlignment: Qt.AlignLeft
            verticalAlignment: Qt.AlignVCenter
        }
    }
}
