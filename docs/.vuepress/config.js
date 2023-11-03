module.exports = {
    title: 'Curly',
    description: 'Curly Documentation',
    base: '/Curly/',
    theme: 'default-prefers-color-scheme',
    themeConfig: {
        logo: '/lil_guy_transparent.png',
        repo: 'evan-bertis-sample/curly',
        editLinkText: 'Help us improve this page!',
        docsRepo: 'evan-bertis-sample/curly',
        docsDir: 'docs',
        docsBranch: 'main',
        editLinks: true,
        overrideTheme: 'dark',
        sidebar: [
            {
                title: 'Introduction',
                collapsable: true,
                children: [
                    '/Introduction/WhatIsCurly.md',
                    '/Introduction/QuickStart.md',
                    '/Introduction/Contributing.md'
                ]
            },
            {
                title: 'API Reference',
                collapsable: false,
                children: [
                    {
                        title: 'Core',
                        collapsable: true,
                        children: [
                            '/API-Reference/Core/CoreOverview.md',
                            '/API-Reference/Core/AudioManagement.md',
                            '/API-Reference/Core/InputManagement.md',
                            '/API-Reference/Core/CustomLogging.md',
                            '/API-Reference/Core/SavingData.md'
                        ]
                    }
                ]
            }
        ]
    }
}