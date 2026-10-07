import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

ToolBar {
    Layout.fillWidth: true

    Connections {
        target: program
        function onFileListUpdated() {
            fileCount.text = program.files.count() + " items";
        }
    }

    Connections {
        target: program

        function onFiltersUpdated() {
            console.log("filtersUpdated");
            filterStateItemsInFolders.filterEnabled = program.filters.itemsInFolders;
            filterStateFoldersAsItems.filterEnabled = program.filters.foldersAsItems;
            filterStateIncludeSubfolders.filterEnabled = program.filters.includeSubfolders;
            filterStateIncludeHidden.filterEnabled = program.filters.includeHidden;
            filterStateSkipRootFolders.filterEnabled = program.filters.skipRootFolders;
        }
    }

    RowLayout {
        FilterButton {
            id: filterStateItemsInFolders
            source: "qrc:/assemblies/JBRenamer/icons/filters_files_in_folders.png"
            tooltip: "Add items within folders"
            filterEnabled: program.filters.itemsInFolders
        }
        FilterButton {
            id: filterStateFoldersAsItems
            source: "qrc:/assemblies/JBRenamer/icons/filters_folders_as_files.png"
            filterEnabled: program.filters.foldersAsItems
            tooltip: "Add folders as items"
        }
        FilterButton {
            id: filterStateIncludeSubfolders
            source: "qrc:/assemblies/JBRenamer/icons/filters_subfolders.png"
            filterEnabled: program.filters.includeSubfolders
            tooltip: "Include subfolders"
        }
        FilterButton {
            id: filterStateIncludeHidden
            source: "qrc:/assemblies/JBRenamer/icons/filters_hidden.png"
            filterEnabled: program.filters.includeHidden
            tooltip: "Include hidden items"
        }
        FilterButton {
            id: filterStateSkipRootFolders
            source: "qrc:/assemblies/JBRenamer/icons/filters_skip_root_folders.png"
            filterEnabled: program.filters.skipRootFolders
            tooltip: "Skip root folders when added as items"
        }

        ToolSeparator {
        }

        Label {
            id: fileCount
            text: program.files.count() + " items"

            horizontalAlignment: Qt.AlignLeft
            verticalAlignment: Qt.AlignVCenter
        }
    }
}
